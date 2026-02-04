# Quick Start Guide - Data Access Layer dengan Dapper

## 📋 Overview
Panduan cepat untuk memulai belajar dan menggunakan materi Data Access Layer dengan Dapper.

## 🎯 Untuk Siapa?
- Developer yang ingin belajar Dapper
- Developer yang ingin memahami Data Access Layer best practices
- Developer yang familiar dengan C# dan SQL
- Mahasiswa/siswa yang belajar .NET development

## 📚 Path Pembelajaran

### 1️⃣ Pahami Teori (30 menit)
Baca dokumentasi lengkap:
- 📖 [README.md - Dokumentasi Lengkap](./README.md)
  - Pengenalan Dapper
  - Result Mapping (semua teknik)
  - Integrasi Arsitektur
  - Query Execution
  - RAW SQL Best Practices

### 2️⃣ Explore Hands-On Project (30 menit)
Jelajahi project yang sudah siap:
- 📂 Lihat struktur project di `hands-on-project/`
- 📖 Baca [Hands-On README](./hands-on-project/README.md)
- 🔍 Review kode di setiap layer:
  - Domain/Entities - lihat model data
  - Domain/Repositories - lihat interface
  - Infrastructure/DataAccess/Repositories - lihat implementasi Dapper
  - API/Controllers - lihat endpoint

### 3️⃣ Jalankan Project (20 menit)
```bash
cd hands-on-project
dotnet restore
dotnet build
dotnet run
```

Akses Swagger UI di browser:
```
https://localhost:7001
atau
http://localhost:5001
```

### 4️⃣ Test API (30 menit)
Gunakan Swagger UI atau Postman untuk test:
1. GET /api/products - Lihat semua produk
2. GET /api/products/1 - Lihat produk tertentu
3. POST /api/products - Buat produk baru
4. PUT /api/products/1 - Update produk
5. GET /api/orders/1/details - Lihat order dengan detail (contoh multi-mapping)

### 5️⃣ Pelajari Kode (60 menit)
Focus pada file-file kunci:
- `ProductRepository.cs` - Implementasi CRUD dengan Dapper
- `OrderRepository.cs` - Multi-mapping dan Transaction
- `DatabaseInitializer.cs` - Setup database dan seed data
- `ProductsController.cs` - API endpoint dengan error handling

### 6️⃣ Kerjakan Assignment (4-8 jam)
- 📝 Baca [ASSIGNMENT.md](./hands-on-project/ASSIGNMENT.md)
- ✅ Kerjakan tugas secara bertahap
- 🧪 Test setiap implementasi
- 📊 Dokumentasikan hasil kerja

## 🔧 Prerequisites

### Software yang Dibutuhkan
1. **.NET 8 SDK**
   ```bash
   dotnet --version  # pastikan >= 8.0
   ```
   Download: https://dotnet.microsoft.com/download

2. **SQL Server atau SQL Server LocalDB**
   - Windows: SQL Server Express atau LocalDB (sudah included di Visual Studio)
   - Mac/Linux: SQL Server di Docker
   
   Docker command:
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
      -p 1433:1433 --name sqlserver \
      -d mcr.microsoft.com/mssql/server:2022-latest
   ```

3. **Code Editor** (pilih salah satu)
   - Visual Studio 2022 (Community/Professional/Enterprise)
   - Visual Studio Code + C# extension
   - JetBrains Rider

4. **API Testing Tool** (pilih salah satu)
   - Swagger UI (sudah included di project)
   - Postman
   - Thunder Client (VS Code extension)
   - curl command

### Optional Tools
- Git (untuk version control)
- SQL Server Management Studio (SSMS) untuk database management
- Azure Data Studio (cross-platform alternative untuk SSMS)

## 🚀 Setup Project

### Windows (LocalDB)
```bash
cd hands-on-project

# Gunakan connection string default di appsettings.json
# "Server=(localdb)\\mssqllocaldb;Database=ProductDB;Trusted_Connection=true;TrustServerCertificate=true"

dotnet restore
dotnet build
dotnet run
```

### Windows/Mac/Linux (SQL Server di Docker)
```bash
cd hands-on-project

# Update connection string di appsettings.json:
# "Server=localhost,1433;Database=ProductDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"

dotnet restore
dotnet build
dotnet run
```

### Windows/Mac/Linux (Remote SQL Server)
```bash
cd hands-on-project

# Update connection string di appsettings.json:
# "Server=your-server;Database=ProductDB;User Id=your-user;Password=your-password;TrustServerCertificate=true"

dotnet restore
dotnet build
dotnet run
```

## 📝 Checklist Pembelajaran

- [ ] Membaca dokumentasi lengkap
- [ ] Memahami konsep Result Mapping
- [ ] Memahami Clean Architecture dengan Dapper
- [ ] Memahami Repository Pattern
- [ ] Setup dan run hands-on project
- [ ] Test semua API endpoints
- [ ] Review kode ProductRepository
- [ ] Review kode OrderRepository (multi-mapping)
- [ ] Memahami Transaction management
- [ ] Memahami Error handling
- [ ] Memahami SQL Injection prevention
- [ ] Kerjakan Assignment Tugas 1
- [ ] Kerjakan Assignment Tugas 2
- [ ] Kerjakan Assignment Tugas 3
- [ ] Kerjakan Assignment Tugas 4

## 💡 Tips Sukses

### Untuk Pemula
1. **Jangan skip teori** - Pahami konsep dulu sebelum coding
2. **Ikuti urutan** - Jangan skip langkah-langkah
3. **Hands-on** - Ketik ulang kode, jangan copy-paste
4. **Bertanya** - Jangan ragu bertanya jika stuck

### Untuk Yang Berpengalaman
1. **Review best practices** - Bandingkan dengan pengalaman Anda
2. **Explore advanced topics** - Coba bonus challenges
3. **Optimize** - Coba improve performa queries
4. **Share knowledge** - Bantu yang lain belajar

## 🐛 Troubleshooting

### Build Error
```bash
# Clear bin/obj folders
rm -rf bin/ obj/
dotnet clean
dotnet restore
dotnet build
```

### Connection Error
1. Pastikan SQL Server running
2. Check connection string di appsettings.json
3. Test connection dengan SSMS/Azure Data Studio
4. Pastikan database ProductDB bisa diakses

### Port Already in Use
Edit `Properties/launchSettings.json`, ganti port:
```json
"applicationUrl": "https://localhost:7002;http://localhost:5002"
```

### Package Restore Error
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

## 📖 Referensi Tambahan

### Official Documentation
- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [Dapper Tutorial](https://www.learndapper.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)

### Video Tutorials (Optional)
- Search YouTube: "Dapper tutorial"
- Search YouTube: "Clean Architecture .NET"
- Search YouTube: "Repository Pattern C#"

### Community
- Stack Overflow - Tag: [dapper]
- Reddit: r/dotnet
- Discord: .NET Community

## 🎓 Setelah Selesai

Setelah menguasai materi ini, Anda bisa:
1. ✅ Implementasi Data Access Layer dengan Dapper
2. ✅ Terapkan Clean Architecture
3. ✅ Buat Web API dengan .NET
4. ✅ Optimasi database queries
5. ✅ Handle transactions dengan benar
6. ✅ Prevent SQL injection
7. ✅ Implement best practices

### Next Learning Path
- Entity Framework Core (alternative ORM)
- CQRS Pattern
- Event Sourcing
- Microservices Architecture
- GraphQL with .NET
- Authentication & Authorization
- Testing (Unit Tests, Integration Tests)

## 📞 Support

Jika ada pertanyaan atau issues:
1. Review dokumentasi lagi
2. Check troubleshooting section
3. Search di Stack Overflow
4. Buka issue di repository
5. Contact instruktur/mentor

---

**Selamat Belajar! 🎉**

> "The only way to learn a new programming language is by writing programs in it." - Dennis Ritchie
