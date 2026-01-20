using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Service;
using static Vehicle_Inspection.Service.IReceiveProfile;

namespace Vehicle_Inspection.Controllers
{
    public class ReceiveProfileController : Controller
    {
        private readonly IReceiveProfile _receiveProfileService;

        public ReceiveProfileController(IReceiveProfile receiveProfileService)
        {
            _receiveProfileService = receiveProfileService;
        }

        /// <summary>
        /// Hiển thị trang Tiếp nhận hồ sơ (View Only)
        /// </summary>
        [Route("receive-profile")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Hiển thị trang Sửa hồ sơ
        /// </summary>
        [Route("receive-profile/edit")]
        public async Task<IActionResult> Edit([FromQuery] string? cccd, [FromQuery] string? plateNo)
        {
            if (string.IsNullOrWhiteSpace(cccd) && string.IsNullOrWhiteSpace(plateNo))
            {
                TempData["ErrorMessage"] = "Vui lòng tìm kiếm hồ sơ trước khi chỉnh sửa";
                return RedirectToAction("Index");
            }

            var result = await _receiveProfileService.SearchAsync(cccd, plateNo);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ";
                return RedirectToAction("Index");
            }

            ViewBag.SearchData = result.Data;
            return View();
        }

        /// <summary>
        /// Hiển thị trang Tạo mới hồ sơ
        /// </summary>
        [Route("receive-profile/create")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// API: Tìm kiếm tổng hợp (theo cả CCCD và Biển số)
        /// </summary>
        [HttpGet]
        [Route("api/receive-profile/search")]
        public async Task<IActionResult> Search([FromQuery] string? cccd, [FromQuery] string? plateNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cccd) && string.IsNullOrWhiteSpace(plateNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập CCCD hoặc Biển số xe"
                    });
                }

                var result = await _receiveProfileService.SearchAsync(cccd, plateNo);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin với dữ liệu đã nhập"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.SearchType == "cccd"
                        ? "Tìm kiếm thành công theo CCCD"
                        : "Tìm kiếm thành công theo biển số xe",
                    searchType = result.SearchType,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tìm kiếm",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// API: Cập nhật thông tin Owner, Vehicle và Specification (có upload ảnh)
        /// </summary>
        [HttpPost]
        [Route("api/receive-profile/update")]
        public async Task<IActionResult> Update()
        {
            try
            {
                Console.WriteLine("=== START UPDATE API ===");

                // Đọc JSON từ form
                var jsonData = Request.Form["jsonData"].ToString();
                Console.WriteLine($"📦 JSON Data length: {jsonData?.Length ?? 0} characters");

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    Console.WriteLine("❌ JSON Data is empty!");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ - JSON rỗng"
                    });
                }

                // Parse JSON thành object
                UpdateProfileRequest? request = null;
                try
                {
                    request = System.Text.Json.JsonSerializer.Deserialize<UpdateProfileRequest>(jsonData, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Console.WriteLine($"✅ Parsed JSON successfully");
                }
                catch (Exception parseEx)
                {
                    Console.WriteLine($"❌ JSON Parse Error: {parseEx.Message}");
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Lỗi parse JSON: {parseEx.Message}"
                    });
                }

                if (request == null)
                {
                    Console.WriteLine("❌ Request object is null after parsing");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ - Parse thất bại"
                    });
                }

                Console.WriteLine($"📋 Owner ID: {request.Owner?.OwnerId}");
                Console.WriteLine($"📋 Owner Name: {request.Owner?.FullName}");
                Console.WriteLine($"📋 Vehicle ID: {request.Vehicle?.VehicleId}");
                Console.WriteLine($"📋 Plate No: {request.Vehicle?.PlateNo}");

                // Validate dữ liệu
                var validationErrors = _receiveProfileService.ValidateProfile(request);
                if (validationErrors.Any())
                {
                    Console.WriteLine($"❌ Validation errors: {string.Join(", ", validationErrors)}");
                    return BadRequest(new
                    {
                        success = false,
                        message = string.Join("<br>", validationErrors)
                    });
                }

                // Upload ảnh (nếu có)
                string? imageUrl = null;
                var profilePicture = Request.Form.Files.GetFile("ProfilePicture");

                if (profilePicture != null && profilePicture.Length > 0)
                {
                    Console.WriteLine($"📷 Uploading image: {profilePicture.FileName} ({profilePicture.Length} bytes)");

                    try
                    {
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "receiveprofile");
                        if (!Directory.Exists(uploads))
                        {
                            Directory.CreateDirectory(uploads);
                            Console.WriteLine($"📁 Created directory: {uploads}");
                        }

                        var fileName = $"{request.Owner.OwnerId}{Path.GetExtension(profilePicture.FileName)}";
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profilePicture.CopyToAsync(stream);
                        }

                        imageUrl = $"/images/receiveprofile/{fileName}";
                        Console.WriteLine($"✅ Image saved: {imageUrl}");
                    }
                    catch (Exception imgEx)
                    {
                        Console.WriteLine($"⚠️ Image upload error: {imgEx.Message}");
                        // Tiếp tục xử lý dù upload ảnh lỗi
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ No image uploaded");
                }

                // Gọi service để cập nhật database
                Console.WriteLine("💾 Calling UpdateProfileAsync...");
                var result = await _receiveProfileService.UpdateProfileAsync(request, imageUrl);
                Console.WriteLine($"✅ Update result: {result}");

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Cập nhật thông tin thành công",
                        imageUrl = imageUrl
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Không thể cập nhật thông tin"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPTION: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// API: Lấy danh sách tỉnh/thành phố
        /// </summary>
        [HttpGet]
        [Route("api/receive-profile/provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            try
            {
                var provinces = await _receiveProfileService.GetProvincesAsync();

                if (provinces == null || provinces.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy dữ liệu tỉnh/thành phố"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = provinces
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tải dữ liệu tỉnh/thành phố",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// API: Lấy danh sách phường/xã theo tỉnh
        /// </summary>
        [HttpGet]
        [Route("api/receive-profile/wards")]
        public async Task<IActionResult> GetWards([FromQuery] string province)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(province))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn tỉnh/thành phố"
                    });
                }

                var wards = await _receiveProfileService.GetWardsByProvinceAsync(province);

                return Ok(new
                {
                    success = true,
                    data = wards
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tải dữ liệu phường/xã",
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        [Route("api/receive-profile/vehicle-types")]
        public async Task<IActionResult> GetVehicleTypes()
        {
            try
            {
                var vehicleTypes = await _receiveProfileService.GetVehicleTypesAsync();

                if (vehicleTypes == null || vehicleTypes.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy dữ liệu loại phương tiện"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = vehicleTypes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tải dữ liệu loại phương tiện",
                    error = ex.Message
                });
            }
        }

        [Route("receive-profile/approve")]
        private async Task<IActionResult> Approve([FromQuery] string? cccd, [FromQuery] string? plateNo)
        {
            if (string.IsNullOrWhiteSpace(cccd) && string.IsNullOrWhiteSpace(plateNo))
            {
                TempData["ErrorMessage"] = "Vui lòng tìm kiếm hồ sơ trước khi xét duyệt";
                return RedirectToAction("Index");
            }

            var result = await _receiveProfileService.SearchAsync(cccd, plateNo);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ";
                return RedirectToAction("Index");
            }

            ViewBag.SearchData = result.Data;
            return View();
        }
    }
}

