import { Link } from 'react-router-dom'

const features = [
  {
    title: 'Gerçek Zamanlı Takip',
    description: 'Alış ve satış işlemlerini anlık kaydedin, stok maliyetlerini otomatik hesaplayın. SignalR ile her değişiklik anında yansır.',
    icon: (
      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
      </svg>
    ),
  },
  {
    title: 'Akıllı Arama',
    description: 'Elasticsearch destekli tam metin arama ile binlerce ürün arasında milisaniyeler içinde istediğinizi bulun.',
    icon: (
      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
      </svg>
    ),
  },
  {
    title: 'Lokasyon Bazlı Depo',
    description: 'Ürünlerin hangi raf veya depoda olduğunu saniyeler içinde görün, lokasyon bazlı filtreleme yapın.',
    icon: (
      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
      </svg>
    ),
  },
  {
    title: 'Görsel Envanter',
    description: 'MinIO ile bulut destekli görsel yükleme. Ürün fotoğraflarını ekleyerek karmaşıklığı önleyin.',
    icon: (
      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
      </svg>
    ),
  },
  {
    title: 'Fiyat Takibi',
    description: 'Her ürün için alış/satış geçmişi, ortalama maliyet ve potansiyel kâr hesaplaması otomatik yapılır.',
    icon: (
      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
      </svg>
    ),
  },
  {
    title: 'Raporlama & Dışa Aktarma',
    description: 'Excel/PDF raporları, kritik stok uyarıları ve Gemini AI destekli doğal dil sorgulaması.',
    icon: (
      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
    ),
  },
]

const categories = [
  { label: 'Akıllı Telefonlar', icon: '📱' },
  { label: 'Tabletler', icon: '📟' },
  { label: 'Bilgisayarlar', icon: '💻' },
  { label: 'Elektronik', icon: '🔌' },
  { label: 'Aksesuar', icon: '🎧' },
  { label: 'Yazıcı & Ofis', icon: '🖨️' },
  { label: 'Ağ Ekipmanı', icon: '📡' },
  { label: 'Diğer', icon: '📦' },
]

const stats = [
  { value: '10K+', label: 'Ürün Kaydı' },
  { value: '99.9%', label: 'Çalışma Süresi' },
  { value: 'Anlık', label: 'Stok Güncelleme' },
  { value: 'Sınırsız', label: 'Kategori' },
]

export default function HomePage() {
  return (
    <div className="min-h-screen bg-gray-50 font-sans text-gray-900">
      {/* Navbar */}
      <nav className="sticky top-0 z-50 bg-white/80 backdrop-blur-md border-b border-gray-100 shadow-sm">
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-3">
              <div className="flex items-center justify-center w-9 h-9 bg-gradient-to-br from-indigo-600 to-indigo-700 rounded-xl shadow-md">
                <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                </svg>
              </div>
              <span className="text-xl font-bold">
                <span className="text-indigo-600">Envan</span>
                <span className="text-gray-800">ter</span>
              </span>
            </div>

            <div className="hidden md:flex items-center space-x-8 text-sm font-medium text-gray-600">
              <a href="#features" className="hover:text-indigo-600 transition-colors">Özellikler</a>
              <a href="#tech" className="hover:text-indigo-600 transition-colors">Teknolojiler</a>
              <a href="#stats" className="hover:text-indigo-600 transition-colors">Hakkında</a>
            </div>

            <Link
              to="/login"
              className="inline-flex items-center px-5 py-2 bg-indigo-600 text-white text-sm font-semibold rounded-lg hover:bg-indigo-700 transition-colors shadow-md"
            >
              Giriş Yap
              <svg className="w-4 h-4 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
              </svg>
            </Link>
          </div>
        </div>
      </nav>

      {/* Hero */}
      <header className="relative overflow-hidden bg-white">
        {/* Arka plan — hafif mesh gradient + blur topları */}
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-indigo-100 via-white to-purple-50" />
        <div className="absolute top-0 left-1/4 w-[500px] h-[500px] bg-indigo-200 rounded-full filter blur-[120px] opacity-30 pointer-events-none" />
        <div className="absolute bottom-0 right-1/4 w-[400px] h-[400px] bg-purple-200 rounded-full filter blur-[100px] opacity-25 pointer-events-none" />
        <div className="absolute top-1/2 left-0 w-[300px] h-[300px] bg-sky-100 rounded-full filter blur-[80px] opacity-30 pointer-events-none" />
        {/* İnce nokta deseni */}
        <div className="absolute inset-0 opacity-[0.03]" style={{ backgroundImage: 'radial-gradient(circle, #6366f1 1px, transparent 1px)', backgroundSize: '32px 32px' }} />

        <div className="relative max-w-7xl mx-auto px-6 lg:px-8 py-24 lg:py-32">
          <div className="flex flex-col lg:flex-row items-center gap-16">
            {/* Left */}
            <div className="lg:w-1/2 space-y-8 text-center lg:text-left">
              <div className="inline-flex items-center px-4 py-1.5 bg-indigo-50 text-indigo-700 text-sm font-medium rounded-full border border-indigo-100">
                <span className="w-2 h-2 bg-indigo-500 rounded-full mr-2 animate-pulse" />
                Gerçek zamanlı envanter yönetimi
              </div>
              <h1 className="text-5xl lg:text-6xl font-extrabold leading-tight tracking-tight">
                Envanter Yönetiminde{' '}
                <span className="bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                  Yeni Nesil Çözüm
                </span>
              </h1>
              <p className="text-lg text-gray-500 max-w-lg mx-auto lg:mx-0">
                Telefon, tablet, PC ve tüm elektronik ürünlerinizin alış-satış takibini, depo lokasyonlarını ve görsel yönetimini tek platformdan profesyonelce yönetin.
              </p>
              <div className="flex flex-col sm:flex-row gap-4 justify-center lg:justify-start">
                <Link
                  to="/login"
                  className="inline-flex items-center justify-center px-8 py-4 bg-indigo-600 text-white font-bold rounded-xl hover:bg-indigo-700 transition-all shadow-lg hover:shadow-indigo-200 hover:shadow-xl"
                >
                  Hemen Başla
                  <svg className="w-5 h-5 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
                  </svg>
                </Link>
                <a
                  href="#features"
                  className="inline-flex items-center justify-center px-8 py-4 border-2 border-gray-200 text-gray-700 font-bold rounded-xl hover:border-indigo-300 hover:bg-indigo-50 transition-all"
                >
                  Özellikleri İncele
                </a>
              </div>
            </div>

            {/* Right — mock dashboard card */}
            <div className="lg:w-1/2 w-full max-w-md mx-auto">
              <div className="relative">
                <div className="bg-gradient-to-br from-indigo-100 to-purple-100 rounded-3xl p-6 transform rotate-2 shadow-inner" />
                <div className="absolute inset-0 bg-white rounded-3xl shadow-2xl border border-gray-100 p-6 transform -rotate-1">
                  {/* Mock header */}
                  <div className="flex items-center justify-between mb-5">
                    <div className="flex items-center space-x-2">
                      <div className="w-8 h-8 bg-indigo-600 rounded-lg" />
                      <div className="h-3 w-24 bg-gray-200 rounded" />
                    </div>
                    <div className="h-6 w-16 bg-green-100 rounded-full" />
                  </div>
                  {/* Mock stats row */}
                  <div className="grid grid-cols-3 gap-3 mb-5">
                    {['bg-indigo-50', 'bg-purple-50', 'bg-green-50'].map((bg, i) => (
                      <div key={i} className={`${bg} rounded-xl p-3`}>
                        <div className="h-2 w-10 bg-gray-300 rounded mb-2" />
                        <div className="h-4 w-14 bg-gray-200 rounded" />
                      </div>
                    ))}
                  </div>
                  {/* Mock table rows */}
                  <div className="space-y-2">
                    {[1, 2, 3].map((i) => (
                      <div key={i} className="flex items-center space-x-3 p-3 bg-gray-50 rounded-xl border border-dashed border-gray-200">
                        <div className="w-8 h-8 bg-gray-200 rounded-lg flex-shrink-0" />
                        <div className="flex-1 space-y-1.5">
                          <div className="h-2.5 bg-gray-200 rounded w-3/4" />
                          <div className="h-2 bg-gray-100 rounded w-1/2" />
                        </div>
                        <div className="h-5 w-12 bg-indigo-100 rounded-full" />
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Features */}
      <section id="features" className="bg-gray-50 py-24">
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-extrabold mb-4">Her İhtiyacınız İçin Hazır</h2>
            <p className="text-gray-500 text-lg max-w-2xl mx-auto">Küçük işletmelerden büyük depolara kadar ölçeklenebilen, modern altyapıyla desteklenen özellikler.</p>
          </div>
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((f) => (
              <div key={f.title} className="bg-white rounded-2xl p-7 border border-gray-100 hover:border-indigo-200 hover:shadow-lg transition-all group">
                <div className="w-12 h-12 bg-indigo-50 rounded-xl flex items-center justify-center text-indigo-600 mb-5 group-hover:bg-indigo-600 group-hover:text-white transition-all">
                  {f.icon}
                </div>
                <h3 className="text-lg font-bold mb-2">{f.title}</h3>
                <p className="text-gray-500 text-sm leading-relaxed">{f.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Tech Stack */}
      <section id="tech" className="bg-white py-24">
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <div className="text-center mb-16">
            <span className="inline-flex items-center px-4 py-1.5 bg-indigo-50 text-indigo-700 text-sm font-medium rounded-full border border-indigo-100 mb-4">
              Mimari & Altyapı
            </span>
            <h2 className="text-4xl font-extrabold mb-4">Teknoloji Yığını</h2>
            <p className="text-gray-500 text-lg max-w-2xl mx-auto">
              Kurumsal düzeyde dayanıklılık için seçilmiş, modern ve kanıtlanmış teknolojiler.
            </p>
          </div>

          {/* Architecture Layers */}
          <div className="grid lg:grid-cols-3 gap-8 mb-14">
            {/* Backend */}
            <div className="relative bg-gradient-to-br from-slate-900 to-slate-800 rounded-2xl p-7 text-white overflow-hidden">
              <div className="absolute top-0 right-0 w-40 h-40 bg-indigo-500 rounded-full filter blur-[80px] opacity-20 pointer-events-none" />
              <div className="relative">
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 bg-indigo-500/20 rounded-xl flex items-center justify-center border border-indigo-400/30">
                    <svg className="w-5 h-5 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01" />
                    </svg>
                  </div>
                  <div>
                    <p className="text-xs text-slate-400 font-medium uppercase tracking-wider">Katman 1</p>
                    <h3 className="text-lg font-bold">Backend</h3>
                  </div>
                </div>
                <div className="space-y-3">
                  {[
                    { name: '.NET 10 / ASP.NET Core', desc: 'Web API & minimal hosting', color: 'bg-purple-500/20 text-purple-300 border-purple-500/30' },
                    { name: 'MediatR — CQRS', desc: 'Komut / sorgu ayrımı', color: 'bg-indigo-500/20 text-indigo-300 border-indigo-500/30' },
                    { name: 'Entity Framework Core', desc: 'ORM & migration', color: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
                    { name: 'ASP.NET Core Identity', desc: 'Kullanıcı & rol yönetimi', color: 'bg-sky-500/20 text-sky-300 border-sky-500/30' },
                    { name: 'JWT Bearer Auth', desc: 'Stateless kimlik doğrulama', color: 'bg-teal-500/20 text-teal-300 border-teal-500/30' },
                    { name: 'SignalR', desc: 'Gerçek zamanlı bildirimler', color: 'bg-green-500/20 text-green-300 border-green-500/30' },
                    { name: 'Hangfire', desc: 'Dayanıklı arka plan işleri', color: 'bg-orange-500/20 text-orange-300 border-orange-500/30' },
                  ].map((t) => (
                    <div key={t.name} className="flex items-center justify-between gap-3">
                      <div>
                        <p className="text-sm font-semibold text-white leading-tight">{t.name}</p>
                        <p className="text-xs text-slate-400 mt-0.5">{t.desc}</p>
                      </div>
                      <span className={`flex-shrink-0 text-xs font-medium px-2 py-0.5 rounded-full border ${t.color}`}>
                        ✓
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Data Layer */}
            <div className="relative bg-gradient-to-br from-slate-900 to-slate-800 rounded-2xl p-7 text-white overflow-hidden">
              <div className="absolute top-0 right-0 w-40 h-40 bg-emerald-500 rounded-full filter blur-[80px] opacity-20 pointer-events-none" />
              <div className="relative">
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 bg-emerald-500/20 rounded-xl flex items-center justify-center border border-emerald-400/30">
                    <svg className="w-5 h-5 text-emerald-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582 4 8 4m0 0v7" />
                    </svg>
                  </div>
                  <div>
                    <p className="text-xs text-slate-400 font-medium uppercase tracking-wider">Katman 2</p>
                    <h3 className="text-lg font-bold">Veri & Depolama</h3>
                  </div>
                </div>
                <div className="space-y-3">
                  {[
                    { name: 'PostgreSQL', desc: 'Ana ilişkisel veritabanı (source of truth)', color: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
                    { name: 'Elasticsearch', desc: 'Tam metin & faceted arama motoru', color: 'bg-yellow-500/20 text-yellow-300 border-yellow-500/30' },
                    { name: 'Redis', desc: 'Nesil tabanlı önbellek (cache invalidation)', color: 'bg-red-500/20 text-red-300 border-red-500/30' },
                    { name: 'MinIO (S3)', desc: 'Ürün görsellerinin nesne deposu', color: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' },
                    { name: 'Bulk Indexing', desc: 'Elasticsearch 500\'erlik chunk\'larla toplu yazma', color: 'bg-orange-500/20 text-orange-300 border-orange-500/30' },
                    { name: 'Cache Invalidation', desc: 'Yazma işleminde nesil anahtarı sıfırlama', color: 'bg-pink-500/20 text-pink-300 border-pink-500/30' },
                    { name: 'EF Core Migrations', desc: 'Şema sürüm yönetimi', color: 'bg-violet-500/20 text-violet-300 border-violet-500/30' },
                  ].map((t) => (
                    <div key={t.name} className="flex items-center justify-between gap-3">
                      <div>
                        <p className="text-sm font-semibold text-white leading-tight">{t.name}</p>
                        <p className="text-xs text-slate-400 mt-0.5">{t.desc}</p>
                      </div>
                      <span className={`flex-shrink-0 text-xs font-medium px-2 py-0.5 rounded-full border ${t.color}`}>
                        ✓
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Frontend & DevOps */}
            <div className="space-y-6">
              {/* Frontend */}
              <div className="relative bg-gradient-to-br from-slate-900 to-slate-800 rounded-2xl p-7 text-white overflow-hidden">
                <div className="absolute bottom-0 left-0 w-32 h-32 bg-cyan-500 rounded-full filter blur-[60px] opacity-20 pointer-events-none" />
                <div className="relative">
                  <div className="flex items-center gap-3 mb-5">
                    <div className="w-10 h-10 bg-cyan-500/20 rounded-xl flex items-center justify-center border border-cyan-400/30">
                      <svg className="w-5 h-5 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                      </svg>
                    </div>
                    <div>
                      <p className="text-xs text-slate-400 font-medium uppercase tracking-wider">Katman 3</p>
                      <h3 className="text-lg font-bold">Frontend</h3>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {[
                      { name: 'React 19', color: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/30' },
                      { name: 'TypeScript', color: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
                      { name: 'Vite', color: 'bg-purple-500/20 text-purple-300 border-purple-500/30' },
                      { name: 'TanStack Query', color: 'bg-red-500/20 text-red-300 border-red-500/30' },
                      { name: 'Tailwind CSS', color: 'bg-teal-500/20 text-teal-300 border-teal-500/30' },
                      { name: 'React Router v7', color: 'bg-orange-500/20 text-orange-300 border-orange-500/30' },
                      { name: 'SignalR Client', color: 'bg-green-500/20 text-green-300 border-green-500/30' },
                    ].map((t) => (
                      <span key={t.name} className={`text-xs font-semibold px-3 py-1 rounded-full border ${t.color}`}>
                        {t.name}
                      </span>
                    ))}
                  </div>
                </div>
              </div>

              {/* DevOps / Infra */}
              <div className="relative bg-gradient-to-br from-slate-900 to-slate-800 rounded-2xl p-7 text-white overflow-hidden">
                <div className="absolute top-0 right-0 w-32 h-32 bg-violet-500 rounded-full filter blur-[60px] opacity-20 pointer-events-none" />
                <div className="relative">
                  <div className="flex items-center gap-3 mb-5">
                    <div className="w-10 h-10 bg-violet-500/20 rounded-xl flex items-center justify-center border border-violet-400/30">
                      <svg className="w-5 h-5 text-violet-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                      </svg>
                    </div>
                    <div>
                      <p className="text-xs text-slate-400 font-medium uppercase tracking-wider">Katman 4</p>
                      <h3 className="text-lg font-bold">DevOps & Altyapı</h3>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {[
                      { name: 'Docker Compose', color: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
                      { name: 'Nginx (prod)', color: 'bg-green-500/20 text-green-300 border-green-500/30' },
                      { name: 'Swagger / OpenAPI', color: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' },
                      { name: 'Git', color: 'bg-orange-500/20 text-orange-300 border-orange-500/30' },
                    ].map((t) => (
                      <span key={t.name} className={`text-xs font-semibold px-3 py-1 rounded-full border ${t.color}`}>
                        {t.name}
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Architecture flow */}
          <div className="bg-gradient-to-r from-slate-50 to-indigo-50 border border-indigo-100 rounded-2xl p-8">
            <h3 className="text-center text-lg font-bold text-gray-800 mb-6">Veri Akışı Mimarisi</h3>
            <div className="flex flex-col sm:flex-row items-center justify-center gap-3 text-sm font-medium flex-wrap">
              {[
                { label: 'React UI', color: 'bg-cyan-100 text-cyan-800 border-cyan-200' },
                { label: 'REST API / SignalR', color: 'bg-indigo-100 text-indigo-800 border-indigo-200' },
                { label: 'MediatR (CQRS)', color: 'bg-purple-100 text-purple-800 border-purple-200' },
                { label: 'Redis Cache', color: 'bg-red-100 text-red-800 border-red-200' },
                { label: 'PostgreSQL', color: 'bg-blue-100 text-blue-800 border-blue-200' },
                { label: 'Elasticsearch', color: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
                { label: 'Hangfire Jobs', color: 'bg-orange-100 text-orange-800 border-orange-200' },
                { label: 'MinIO (S3)', color: 'bg-emerald-100 text-emerald-800 border-emerald-200' },
              ].map((item, i, arr) => (
                <div key={item.label} className="flex items-center gap-3">
                  <span className={`px-3 py-1.5 rounded-lg border font-semibold text-xs ${item.color}`}>
                    {item.label}
                  </span>
                  {i < arr.length - 1 && (
                    <svg className="w-4 h-4 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
                    </svg>
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section id="stats" className="bg-indigo-600">
        <div className="max-w-7xl mx-auto px-6 lg:px-8 py-14">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
            {stats.map((s) => (
              <div key={s.label}>
                <div className="text-3xl font-extrabold text-white mb-1">{s.value}</div>
                <div className="text-indigo-200 text-sm font-medium">{s.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Categories */}
      <section id="categories" className="bg-white py-24">
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-extrabold mb-4">Geniş Ürün Yelpazesi</h2>
            <p className="text-gray-500 text-lg">Tüm elektronik kategorilerini tek sistemde yönetin.</p>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-5">
            {categories.map((c) => (
              <div key={c.label} className="flex flex-col items-center p-6 border border-gray-100 rounded-2xl hover:border-indigo-300 hover:shadow-md hover:bg-indigo-50 transition-all cursor-default group">
                <span className="text-4xl mb-3">{c.icon}</span>
                <span className="font-semibold text-sm text-gray-700 group-hover:text-indigo-700">{c.label}</span>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-gradient-to-r from-indigo-600 to-purple-600 py-20">
        <div className="max-w-3xl mx-auto px-6 text-center">
          <h2 className="text-4xl font-extrabold text-white mb-4">Hemen Kullanmaya Başlayın</h2>
          <p className="text-indigo-100 text-lg mb-8">Saniyeler içinde giriş yapın, stoklarınızı profesyonelce yönetin.</p>
          <Link
            to="/login"
            className="inline-flex items-center px-10 py-4 bg-white text-indigo-700 font-bold rounded-xl hover:bg-indigo-50 transition-all shadow-xl text-lg"
          >
            Giriş Yap
            <svg className="w-5 h-5 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
            </svg>
          </Link>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 text-gray-400 py-10">
        <div className="max-w-7xl mx-auto px-6 lg:px-8 flex flex-col md:flex-row justify-between items-center gap-4 text-sm">
          <div className="flex items-center space-x-2">
            <div className="w-7 h-7 bg-indigo-600 rounded-lg flex items-center justify-center">
              <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
            </div>
            <span className="font-semibold text-white">Envanter</span>
          </div>
          <p>© {new Date().getFullYear()} Envanter. Tüm hakları saklıdır.</p>
        </div>
      </footer>
    </div>
  )
}
