📧 Project2Email - AI Destekli Akıllı E-Posta Yönetim Sistemi

Project2Email, geleneksel e-posta yönetimini bir adım öteye taşıyarak Yapay Zeka (AI) ve Gelişmiş Güvenlik katmanlarını tek bir platformda birleştiren modern bir web uygulamasıdır. Proje, sadece mesaj iletimi değil, verinin anlamlandırılması ve güvenli bir kullanıcı deneyimi üzerine inşa edilmiştir.

🚀 Proje Mimarisi ve Uygulanan Çözümler
1. 🤖 Google Gemini AI ile Akıllı Analiz
Sistemin kalbinde yer alan Gemini Pro API entegrasyonu sayesinde, gönderilen her mail manuel müdahale olmaksızın analiz edilir:

   -Otomatik Kategorizasyon: Yazılan mesajın içeriği AI tarafından okunur ve konusuna göre İş, Sosyal, Finans, Tanıtım veya Önemli kategorilerinden birine atanır.

   -Akıllı Özetleme (AI Summary): Uzun maillerin ana fikrini yansıtan, maksimum 10 kelimelik kısa ve öz "Summary" alanları AI tarafından oluşturularak veritabanına kaydedilir.

2. 🔐 Katı Güvenlik ve E-Posta Doğrulama
Kullanıcı güvenliği en üst düzeyde tutulmuştur:

   -Mail Onay Mekanizması: Kullanıcı kayıt olduktan sonra hesabı otomatik olarak pasif durumda kalır. Sisteme giriş yapabilmesi için mail adresine gönderilen 6 haneli doğrulama kodunu onaylaması zorunludur. Onaylanmamış hesaplar dashboard'a erişemez.

   -Gerçek SMTP Entegrasyonu: Şifre sıfırlama, kayıt onaylama ve bilgilendirme mailleri gerçek bir SMTP sunucusu üzerinden kullanıcıların gerçek e-posta adreslerine iletilir.

   -Identity Güvenliği: Şifreler hash'lenmiş olarak saklanır, yetkisiz erişimler Role-Based Authorization ile engellenir.

3. 📊 Dinamik Dashboard ve Veri Görselleştirme
Kullanıcının mail trafiği Chart.js entegrasyonu ile dashboard üzerinde canlı olarak raporlanır:

   -Trafik Analizi: Son 7 güne ait mesajlaşma yoğunluğu çizgi grafik (Line Chart) ile gösterilir.

   -Kategori Dağılımı: AI'ın belirlediği kategorilerin toplam içindeki oranı dinamik Doughnut Chart ile görselleştirilir.

   -Canlı İstatistikler: Yıldızlı mesaj sayısı, en aktif gönderici ve okunmamış mesajlar gibi kritik veriler anlık olarak hesaplanır.

4. 🎨 Next-Gen UI/UX Deneyimi
Uygulama, standart bir yönetim panelinden ziyade modern bir "Application" hissi verir:
  
   -Three.js 3D Background: Giriş, Kayıt ve Doğrulama sayfalarında kullanıcıyı fare hareketlerine duyarlı 3D interaktif modeller karşılar.

   -Glassmorphism Tasarımı: Arka plan bulanıklığı (backdrop-filter) ve koyu tema (Dark Mode) üzerine kurulu, göz yormayan profesyonel arayüz.

   -Sidebar ViewComponent: Sayfa yenilense dahi sayaçların (Gelen kutusu sayısı vb.) her zaman güncel kalmasını sağlayan ViewComponent mimarisi.

⚙️ Teknik Stack

   -Backend: ASP.NET Core 8.0 MVC, Entity Framework Core.

   -AI Service: Google Generative AI (Gemini Service).

   -Database: MSSQL (Relational Database Design).

   -Frontend: Bootstrap 5, Javascript, Three.js, Chart.js.

   -Security: ASP.NET Identity, MailKit (SMTP Server).

🛠️ Veritabanı İlişkileri
Sistemde mesajlar ve kullanıcı durumları birbirinden ayrıştırılarak performansı artırılmıştır. UserMessageState tablosu sayesinde bir mesajın okunma durumu, yıldızlı olup olmaması veya çöp kutusunda bulunması, mesajın kendisinden bağımsız olarak kullanıcı bazlı yönetilir.

⭐ Bu proje, yapay zekanın gündelik yönetim araçlarına nasıl entegre edilebileceğinin bir örneği olarak geliştirilmiştir.
