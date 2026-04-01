import type { ReactNode } from 'react'

declare function ThemeProvider(props: { children: ReactNode }): JSX.Element
export default ThemeProvider

export function useThemeProvider(): {
  currentTheme: string
  changeCurrentTheme: (theme: string) => void
}
