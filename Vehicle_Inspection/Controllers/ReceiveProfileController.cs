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
        /// Hiển thị trang Tiếp nhận hồ sơ
        /// </summary>
        [Route("receive-profile")]
        public IActionResult Index()
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
        /// API: Cập nhật thông tin Owner, Vehicle và Specification
        /// </summary>
        [HttpPut]
        [Route("api/receive-profile/update")]
        public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ"
                    });
                }

                // Validate trên server
                var validationErrors = _receiveProfileService.ValidateProfile(request);
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = string.Join("<br>", validationErrors)
                    });
                }

                var result = await _receiveProfileService.UpdateProfileAsync(request);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Cập nhật thông tin thành công"
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
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật",
                    error = ex.Message
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

                if (provinces == null)
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
    }
}