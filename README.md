# Vehicle_Inspection
Hệ thông đăng kiểm xe tại trung tâm X
// Chuỗi database first
Scaffold-DbContext "Server=vehicle.database.windows.net;Database=VehicleInspectionCenter;User ID=vehicle;Password=Minh21032005@;Encrypt=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data -Context VehInsContext -UseDatabaseNames -NoOnConfiguring -DataAnnotations -Force