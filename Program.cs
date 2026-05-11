using System;
using Microsoft.Data.SqlClient; // SqlConnection, SqlCommand burada

namespace MSSQLOrnegi
{
    class Program
    {
        // -------------------------------------------------------
        // BAĞLANTI DİZESİ — Tek yerden yönetmek için burada tanımlıyoruz
        // Kendi sunucu ve veritabanı adınıza göre değiştirin
        // -------------------------------------------------------
        static string baglantiDizesi =
            "Server=DUYGUKPLNLR\\SQLEXPRESS;Database=ProgramlamaII;Trusted_Connection=True;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            Console.WriteLine("=== MSSQL C# Örneği ===\n");

            // Sırasıyla tüm işlemleri çağırıyoruz
            TabloyuOlustur();
            VeriEkle();
            TumVerileriGetir();
            KosullaGetir();

            Console.WriteLine("\nProgram tamamlandı. Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }


        // ============================================================
        // 1. TABLO OLUŞTURMA
        // ============================================================
        static void TabloyuOlustur()
        {
            Console.WriteLine("-- Tablo oluşturuluyor...");

            // Tablo zaten varsa silip yeniden oluşturuyoruz
            // IF OBJECT_ID → "Bu isimde bir tablo var mı?" kontrolü
            string sql = @"
                IF OBJECT_ID('Ogrenciler', 'U') IS NOT NULL
                    DROP TABLE Ogrenciler;

                CREATE TABLE Ogrenciler
                (
                    ID      INT             PRIMARY KEY IDENTITY(1,1),
                    Ad      NVARCHAR(50),
                    Soyad   NVARCHAR(50),
                    Yas     INT,
                    Sehir   NVARCHAR(50)
                );
            ";

            // using bloğu → işlem bitince bağlantıyı otomatik kapatır
            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open(); // Veritabanına bağlan

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.ExecuteNonQuery();
                    // ExecuteNonQuery → Veri döndürmeyen sorgular için kullanılır
                    // (CREATE, INSERT, UPDATE, DELETE)
                }
            }
            // using bloğu kapandığında bağlantı otomatik kapatılır

            Console.WriteLine("   ✓ Tablo başarıyla oluşturuldu.\n");
        }


        // ============================================================
        // 2. VERİ EKLEME — INSERT
        // ============================================================
        static void VeriEkle()
        {
            Console.WriteLine("-- Veriler ekleniyor...");

            // Ekleyeceğimiz öğrencileri bir dizi içinde tutuyoruz
            // Her öğrenci: Ad, Soyad, Yaş, Şehir
            var ogrenciler = new[]
            {
                new { Ad = "Ahmet",  Soyad = "Yılmaz", Yas = 22, Sehir = "İstanbul" },
                new { Ad = "Ayşe",   Soyad = "Kaya",   Yas = 20, Sehir = "Ankara"   },
                new { Ad = "Mehmet", Soyad = "Demir",  Yas = 25, Sehir = "İzmir"    },
                new { Ad = "Zeynep", Soyad = "Çelik",  Yas = 21, Sehir = "Bursa"    },
                new { Ad = "Ali",    Soyad = "Şahin",  Yas = 23, Sehir = "Antalya"  },
                new { Ad = "Duygu",  Soyad = "Kaplanlar", Yas =21, Sehir = "Eskişehir" }
            };

            // SQL sorgusunda @Ad, @Soyad gibi parametreler kullanıyoruz
            // Bu yönteme "Parametreli Sorgu" denir
            // SQL Injection saldırılarını önlemek için ŞART!
            string sql = @"
                INSERT INTO Ogrenciler (Ad, Soyad, Yas, Sehir)
                VALUES (@Ad, @Soyad, @Yas, @Sehir)
            ";

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                // Her öğrenci için aynı bağlantı üzerinden INSERT yapıyoruz
                foreach (var ogr in ogrenciler)
                {
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        // Parametrelere değer atıyoruz
                        // @Ad yazan yere "Ahmet", @Yas yazan yere 22 gibi...
                        komut.Parameters.AddWithValue("@Ad", ogr.Ad);
                        komut.Parameters.AddWithValue("@Soyad", ogr.Soyad);
                        komut.Parameters.AddWithValue("@Yas", ogr.Yas);
                        komut.Parameters.AddWithValue("@Sehir", ogr.Sehir);

                        komut.ExecuteNonQuery(); // INSERT çalıştır
                    }

                    Console.WriteLine($"   ✓ {ogr.Ad} {ogr.Soyad} eklendi.");
                }
            }

            Console.WriteLine();
        }


        // ============================================================
        // 3. TÜM VERİLERİ GETİRME — SELECT *
        // ============================================================
        static void TumVerileriGetir()
        {
            Console.WriteLine("-- Tüm öğrenciler listeleniyor...");
            Console.WriteLine("   {0,-5} {1,-10} {2,-10} {3,-5} {4,-10}",
                              "ID", "Ad", "Soyad", "Yaş", "Şehir");
            Console.WriteLine("   " + new string('-', 45));

            string sql = "SELECT * FROM Ogrenciler ORDER BY ID";

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    // ExecuteReader → Veri döndüren sorgular için kullanılır
                    // (SELECT sorguları)
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        // okuyucu.Read() → Sıradaki satıra geç
                        // Satır kalmadığında false döner, döngü biter
                        while (okuyucu.Read())
                        {
                            // Sütun adıyla değeri okuyoruz
                            int id = okuyucu.GetInt32(okuyucu.GetOrdinal("ID"));
                            string ad = okuyucu.GetString(okuyucu.GetOrdinal("Ad"));
                            string soyad = okuyucu.GetString(okuyucu.GetOrdinal("Soyad"));
                            int yas = okuyucu.GetInt32(okuyucu.GetOrdinal("Yas"));
                            string sehir = okuyucu.GetString(okuyucu.GetOrdinal("Sehir"));

                            Console.WriteLine("   {0,-5} {1,-10} {2,-10} {3,-5} {4,-10}",
                                              id, ad, soyad, yas, sehir);
                        }
                    }
                }
            }

            Console.WriteLine();
        }


        // ============================================================
        // 4. KOŞULLA VERİ GETİRME — SELECT + WHERE
        // ============================================================
        static void KosullaGetir()
        {
            int araYas = 21; // 21 yaşından büyükleri getireceğiz
            Console.WriteLine($"-- {araYas} yaşından büyük öğrenciler:");

            // WHERE koşulunda da parametre kullanıyoruz
            string sql = @"
                SELECT Ad, Soyad, Yas, Sehir 
                FROM Ogrenciler
                WHERE Yas > @Yas
                ORDER BY Yas ASC
            ";

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    // WHERE'deki @Yas parametresine değer veriyoruz
                    komut.Parameters.AddWithValue("@Yas", araYas);

                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            string ad = okuyucu["Ad"].ToString();
                            string soyad = okuyucu["Soyad"].ToString();
                            int yas = Convert.ToInt32(okuyucu["Yas"]);
                            string sehir = okuyucu["Sehir"].ToString();

                            // okuyucu["SütunAdı"] → alternatif okuma yöntemi
                            Console.WriteLine($"   → {ad} {soyad} | Yaş: {yas} | Şehir: {sehir}");
                        }
                    }
                }
            }

            Console.WriteLine();
        }
    }
}
