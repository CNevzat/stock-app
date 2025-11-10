import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.stockapp.mobile',
  appName: 'Stock App',
  webDir: 'dist',
  server: {
    // Development için - production'da kaldırılmalı veya yorum satırı yapılmalı
    // androidScheme: 'https',
    // url: 'http://localhost:5173',
    // cleartext: true
  },
  android: {
    buildOptions: {
      keystorePath: undefined,
      keystorePassword: undefined,
      keystoreAlias: undefined,
      keystoreAliasPassword: undefined,
      keystoreType: undefined
    }
  }
};

export default config;
