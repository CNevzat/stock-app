import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '../services/authService';

export default function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showPasswordChange, setShowPasswordChange] = useState(false);
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordChangeError, setPasswordChangeError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = await authService.login({ email, password });
      
      // Always save tokens first
      authService.setTokens(response.accessToken, response.refreshToken);
      authService.setUser(response.user);
      
      // Check if password change is required
      if (response.user.mustChangePassword) {
        setShowPasswordChange(true);
        setIsLoading(false);
        return;
      }

      navigate('/');
    } catch (err: any) {
      setError(err.message || 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
    } finally {
      setIsLoading(false);
    }
  };

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordChangeError('');

    if (newPassword !== confirmPassword) {
      setPasswordChangeError('Şifreler eşleşmiyor');
      return;
    }

    if (newPassword.length < 6) {
      setPasswordChangeError('Şifre en az 6 karakter olmalıdır');
      return;
    }

    try {
      const token = authService.getToken();
      if (!token) {
        setPasswordChangeError('Oturum bulunamadı. Lütfen tekrar giriş yapın.');
        return;
      }

      // Use force change password (no current password required)
      await authService.forceChangePassword(
        { newPassword, confirmPassword },
        token
      );
      
      // Refresh user info from server
      const refreshedUser = await authService.getCurrentUser();
      authService.setUser(refreshedUser);
      setShowPasswordChange(false);
      navigate('/');
    } catch (err: any) {
      setPasswordChangeError(err.message || 'Şifre değiştirme başarısız');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
      <div className="max-w-md w-full bg-white rounded-3xl shadow-xl overflow-hidden">
        {/* Header band */}
        <div className="bg-gradient-to-br from-indigo-600 to-indigo-700 px-10 py-10 text-center">
          <div className="mx-auto flex items-center justify-center w-14 h-14 bg-white/20 rounded-2xl mb-4">
            <svg className="w-7 h-7 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
          </div>
          <h2 className="text-3xl font-bold text-white">Tekrar Hoş Geldiniz</h2>
          <p className="text-indigo-200 mt-1 text-sm uppercase tracking-widest">Yönetim Paneline Erişim</p>
        </div>

        <form className="px-10 pt-10 pb-6 space-y-6" onSubmit={handleSubmit}>
          {error && (
            <div className="rounded-xl bg-red-50 border border-red-100 p-4 flex items-start space-x-3">
              <svg className="h-5 w-5 text-red-400 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              <p className="text-sm font-medium text-red-800">{error}</p>
            </div>
          )}

          <div>
            <label htmlFor="email" className="block text-sm font-semibold text-gray-700 mb-2">
              E-posta Adresi
            </label>
            <input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-3 rounded-xl border border-gray-200 focus:ring-2 focus:ring-indigo-500 focus:border-transparent outline-none transition text-sm"
              placeholder="admin@stockapp.com"
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-semibold text-gray-700 mb-2">
              Şifre
            </label>
            <input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-3 rounded-xl border border-gray-200 focus:ring-2 focus:ring-indigo-500 focus:border-transparent outline-none transition text-sm"
              placeholder="••••••••"
            />
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full bg-indigo-600 text-white py-4 rounded-xl font-bold hover:bg-indigo-700 shadow-lg active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
          >
            {isLoading ? (
              <>
                <svg className="animate-spin mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                </svg>
                Giriş yapılıyor...
              </>
            ) : (
              'Sisteme Giriş Yap'
            )}
          </button>

          <p className="text-center text-xs text-gray-400 bg-gray-50 rounded-lg py-2 px-3">
            <span className="font-medium text-gray-500">Demo:</span> admin@stockapp.com / Admin123!
          </p>
        </form>

        <div className="pb-8 px-10 space-y-3 text-center">
          <Link
            to="/"
            className="inline-flex items-center justify-center w-full gap-2 py-2.5 text-sm font-semibold text-gray-600 bg-gray-50 border border-gray-200 rounded-xl hover:bg-gray-100 hover:text-indigo-600 transition-colors"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            Ana Sayfaya Dön
          </Link>
          <p className="text-gray-400 text-sm">
            Erişim sorunu mu yaşıyorsunuz?{' '}
            <a href="mailto:ncirpicioglu@gmail.com?subject=Envanter%20Erişim%20Sorunu" className="text-indigo-600 font-semibold hover:underline">
              Destek Alın
            </a>
          </p>
        </div>
      </div>

      {/* Password Change Modal */}
      {showPasswordChange && (
          <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
              <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" />
              <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
                <div>
                  <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-yellow-100">
                    <svg className="h-6 w-6 text-yellow-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                  </div>
                  <div className="mt-3 text-center sm:mt-5">
                    <h3 className="text-lg font-semibold leading-6 text-gray-900">
                      Şifre Değiştirme Gerekli
                    </h3>
                    <div className="mt-2">
                      <p className="text-sm text-gray-500">
                        İlk girişinizde şifrenizi değiştirmeniz gerekmektedir. Lütfen yeni bir şifre belirleyin.
                      </p>
                    </div>
                  </div>
                </div>
                <form onSubmit={handlePasswordChange} className="mt-5 space-y-4">
                  {passwordChangeError && (
                    <div className="rounded-md bg-red-50 p-3">
                      <p className="text-sm text-red-800">{passwordChangeError}</p>
                    </div>
                  )}
                  <div>
                    <label htmlFor="newPassword" className="block text-sm font-medium text-gray-700">
                      Yeni Şifre
                    </label>
                    <input
                      id="newPassword"
                      type="password"
                      required
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      placeholder="En az 6 karakter"
                    />
                  </div>
                  <div>
                    <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">
                      Şifre Tekrar
                    </label>
                    <input
                      id="confirmPassword"
                      type="password"
                      required
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      placeholder="Şifreyi tekrar girin"
                    />
                  </div>
                  <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                    <button
                      type="submit"
                      className="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 sm:col-start-2"
                    >
                      Şifreyi Değiştir
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setShowPasswordChange(false);
                        setNewPassword('');
                        setConfirmPassword('');
                        setPasswordChangeError('');
                      }}
                      className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                    >
                      İptal
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
      )}
    </div>
  );
}

