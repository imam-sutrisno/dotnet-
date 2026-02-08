# Presentasi Data Access Layer

## 📊 File Presentasi yang Tersedia

Dokumentasi Data Access Layer dengan Dapper telah dikonversi ke dalam format presentasi untuk memudahkan pembelajaran dan sharing:

1. **PowerPoint Presentation** - `Data-Access-Layer-Presentation.pptx`
   - Format: Microsoft PowerPoint (.pptx)
   - Ukuran: ~58KB
   - Ideal untuk: Presentasi interaktif, workshop, dan training

2. **PDF Presentation** - `Data-Access-Layer-Presentation.pdf`
   - Format: Portable Document Format (.pdf)
   - Ukuran: ~23KB
   - Ideal untuk: Distribusi, print-out, dan viewing universal

## 📖 Isi Presentasi

Presentasi mencakup semua topik utama dari README.md:

### 1. Pengenalan
- Apa itu Data Access Layer
- Apa itu Dapper
- Keunggulan Dapper
- Instalasi

### 2. Result Mapping
- Basic Mapping
- Custom Column Mapping
- Multi-Mapping (One-to-One)
- Multi-Mapping (One-to-Many)
- Dynamic Mapping

### 3. Integrasi Arsitektur
- Clean Architecture dengan Dapper
- Repository Pattern
- Unit of Work Pattern
- Dependency Injection

### 4. Query Execution
- Query Methods (Query, QuerySingle, QueryFirst, Execute, ExecuteScalar)
- Parameterized Queries
- Bulk Operations
- Multiple Result Sets
- Stored Procedures

### 5. RAW SQL Best Practice
- SQL Injection Prevention
- Query Optimization
- Transaction Management
- Connection Management
- Error Handling
- Async/Await Best Practice
- Performance Tips

## 🚀 Cara Menggunakan

### PowerPoint (.pptx)
1. Buka file dengan Microsoft PowerPoint, Google Slides, atau LibreOffice Impress
2. Edit sesuai kebutuhan presentasi Anda
3. Gunakan untuk training, workshop, atau presentasi tim

### PDF (.pdf)
1. Buka dengan aplikasi PDF reader (Adobe Reader, browser, dll)
2. Print untuk handout atau dokumentasi offline
3. Share via email atau platform sharing lainnya

## 🔄 Regenerate Presentasi

Jika README.md di-update dan Anda ingin regenerate presentasi:

### Prerequisites
```bash
# Install Python dependencies
pip install python-pptx markdown2 reportlab
```

### Cara Regenerate
```bash
# Jalankan script generator
python3 generate_presentations.py
```

Script akan:
1. Parse README.md
2. Generate file PowerPoint baru
3. Generate file PDF baru
4. Menimpa file lama dengan versi terbaru

## 📝 Catatan

- Presentasi di-generate secara otomatis dari README.md
- Untuk konten yang lebih detail, selalu rujuk ke README.md asli
- Code examples di presentasi mungkin dipotong untuk keterbacaan
- Untuk hands-on practice, lihat folder [hands-on-project](./hands-on-project/)

## 🛠️ Customization

Untuk mengcustomize presentasi:

1. Edit file `generate_presentations.py`
2. Sesuaikan:
   - Style dan warna
   - Layout slide
   - Font size
   - Jumlah konten per slide
3. Jalankan ulang script untuk generate presentasi baru

## 📞 Support

Jika ada pertanyaan atau issue terkait presentasi:
1. Check README.md untuk konten lengkap
2. Review script generator untuk memahami proses
3. Buat issue jika menemukan bug dalam script

---

**Selamat belajar! 🚀**
