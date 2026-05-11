using System;
using Microsoft.Data.SqlClient; 

namespace MSSQLOrnegi
{
    class Program
    {
        
        static string baglantiDizesi =
            "Server=DUYGUKPLNLR\\SQLEXPRESS;Database=ProgramlamaII;Trusted_Connection=True;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            Console.WriteLine("=== MSSQL C# Örneği ===\n");

            
            TabloyuOlustur();
            VeriEkle();
            TumVerileriGetir();
            KosullaGetir();

            Console.WriteLine("\nProgram tamamlandı. Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }


     
        static void TabloyuOlustur()
        {
            Console.WriteLine("-- Tablo oluşturuluyor...");

            
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

            
            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open(); 

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.ExecuteNonQuery();
                    
                }
            }
            

            Console.WriteLine("   ✓ Tablo başarıyla oluşturuldu.\n");
        }


        
        static void VeriEkle()
        {
            Console.WriteLine("-- Veriler ekleniyor...");

            
            var ogrenciler = new[]
            {
                new { Ad = "Ahmet",  Soyad = "Yılmaz", Yas = 22, Sehir = "İstanbul" },
                new { Ad = "Ayşe",   Soyad = "Kaya",   Yas = 20, Sehir = "Ankara"   },
                new { Ad = "Mehmet", Soyad = "Demir",  Yas = 25, Sehir = "İzmir"    },
                new { Ad = "Zeynep", Soyad = "Çelik",  Yas = 21, Sehir = "Bursa"    },
                new { Ad = "Ali",    Soyad = "Şahin",  Yas = 23, Sehir = "Antalya"  },
                new { Ad = "Duygu",  Soyad = "Kaplanlar", Yas =21, Sehir = "Eskişehir" }
            };

           
            string sql = @"
                INSERT INTO Ogrenciler (Ad, Soyad, Yas, Sehir)
                VALUES (@Ad, @Soyad, @Yas, @Sehir)
            ";

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                
                foreach (var ogr in ogrenciler)
                {
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                       
                        komut.Parameters.AddWithValue("@Ad", ogr.Ad);
                        komut.Parameters.AddWithValue("@Soyad", ogr.Soyad);
                        komut.Parameters.AddWithValue("@Yas", ogr.Yas);
                        komut.Parameters.AddWithValue("@Sehir", ogr.Sehir);

                        komut.ExecuteNonQuery(); 
                    }

                    Console.WriteLine($"   ✓ {ogr.Ad} {ogr.Soyad} eklendi.");
                }
            }

            Console.WriteLine();
        }


        
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
                    
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                       
                        while (okuyucu.Read())
                        {
                           
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


      
        static void KosullaGetir()
        {
            int araYas = 21; 
            Console.WriteLine($"-- {araYas} yaşından büyük öğrenciler:");

          
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
                   
                    komut.Parameters.AddWithValue("@Yas", araYas);

                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            string ad = okuyucu["Ad"].ToString();
                            string soyad = okuyucu["Soyad"].ToString();
                            int yas = Convert.ToInt32(okuyucu["Yas"]);
                            string sehir = okuyucu["Sehir"].ToString();

                         
                            Console.WriteLine($"   → {ad} {soyad} | Yaş: {yas} | Şehir: {sehir}");
                        }
                    }
                }
            }

            Console.WriteLine();
        }
    }
}
