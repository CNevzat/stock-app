import { useEffect, useMemo, useRef, useState } from 'react'
import { chatService } from '../services/chatService'

type ChatMessage = {
  id: string
  from: 'user' | 'bot'
  text: string
}

export function ChatWidget() {
  const [isOpen, setIsOpen] = useState(false)
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      id: crypto.randomUUID(),
      from: 'bot',
      text: 'Merhaba! Ben Stock App Asistanıyım. Stok verileri, raporlar veya uygulamayı kullanma hakkında sorularını memnuniyetle yanıtlarım.',
    },
  ])
  const [inputValue, setInputValue] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [suggestions, setSuggestions] = useState<string[]>([
    'Geçen ay en kârlı kategori hangisiydi?',
    'Son 7 gündeki stok girişlerini özetle',
    'Ürün ekleme adımları nelerdir?',
  ])
  const messagesEndRef = useRef<HTMLDivElement | null>(null)

  const handleToggle = () => setIsOpen((prev) => !prev)

  const handleSend = async (question: string) => {
    const trimmed = question.trim()
    if (!trimmed) return

    const userMessage: ChatMessage = {
      id: crypto.randomUUID(),
      from: 'user',
      text: trimmed,
    }
    setMessages((prev) => [...prev, userMessage])
    setInputValue('')

    setIsLoading(true)
    try {
      const response = await chatService.ask(trimmed)
      const botMessage: ChatMessage = {
        id: crypto.randomUUID(),
        from: 'bot',
        text: response.answer,
      }
      setMessages((prev) => [...prev, botMessage])
      setSuggestions(response.suggestions ?? [])
    } catch (error: any) {
      const botMessage: ChatMessage = {
        id: crypto.randomUUID(),
        from: 'bot',
        text: error?.message || 'Şu anda yanıt veremiyorum. Lütfen biraz sonra tekrar dene.',
      }
      setMessages((prev) => [...prev, botMessage])
    } finally {
      setIsLoading(false)
    }
  }

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    handleSend(inputValue)
  }

  const suggestionButtons = useMemo(() => suggestions.slice(0, 3), [suggestions])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, isLoading])

  return (
    <>
      <button
        type="button"
        onClick={handleToggle}
        className="fixed bottom-6 right-6 z-40 flex items-center gap-2 rounded-full bg-gradient-to-r from-indigo-600 to-indigo-700 px-4 py-3 text-sm font-semibold text-white shadow-lg hover:from-indigo-700 hover:to-indigo-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
      >
        <span className="inline-flex h-8 w-8 items-center justify-center rounded-full bg-indigo-500">
          🤖
        </span>
        <span>Stock App Asistan</span>
      </button>

      {isOpen && (
        <div className="fixed bottom-24 right-6 z-40 w-96 max-w-[90vw] rounded-2xl bg-white shadow-2xl ring-1 ring-black/10">
          <div className="flex items-center justify-between rounded-t-2xl bg-indigo-600 px-4 py-3 text-white">
            <div className="flex flex-col">
              <span className="text-sm font-semibold">Stock App Asistan</span>
              <span className="text-xs text-indigo-100">Yapay zekâ destekli stok danışmanı</span>
            </div>
            <button
              onClick={handleToggle}
              className="rounded-full p-2 transition hover:bg-indigo-500/50"
            >
              ✕
            </button>
          </div>

          <div className="flex h-80 flex-col gap-3 overflow-y-auto bg-indigo-50/40 px-4 py-3">
            {messages.map((message) => (
              <div
                key={message.id}
                className={`flex ${message.from === 'user' ? 'justify-end' : 'justify-start'}`}
              >
                <div
                  className={`max-w-[80%] whitespace-pre-wrap rounded-2xl px-3 py-2 text-sm shadow-sm ${
                    message.from === 'user'
                      ? 'bg-indigo-600 text-white'
                      : 'bg-white text-gray-900 ring-1 ring-gray-200'
                  }`}
                >
                  {message.text}
                </div>
              </div>
            ))}

            {isLoading && (
              <div className="flex items-center gap-2 text-xs text-gray-500">
                <span className="inline-flex h-2 w-2 animate-pulse rounded-full bg-indigo-500"></span>
                <span>Yanıt hazırlanıyor...</span>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {suggestionButtons.length > 0 && (
            <div className="flex flex-wrap gap-2 px-4 pb-2">
              {suggestionButtons.map((suggestion) => (
                <button
                  key={suggestion}
                  onClick={() => handleSend(suggestion)}
                  className="rounded-full border border-indigo-200 px-3 py-1 text-xs font-medium text-indigo-700 transition hover:bg-indigo-100"
                >
                  {suggestion}
                </button>
              ))}
            </div>
          )}

          <form onSubmit={handleSubmit} className="flex items-center gap-2 border-t border-gray-200 px-4 py-3">
            <input
              type="text"
              value={inputValue}
              onChange={(event) => setInputValue(event.target.value)}
              placeholder="Sorunu buraya yaz..."
              className="flex-1 rounded-full border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
              disabled={isLoading}
            />
            <button
              type="submit"
              className="rounded-full bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700 disabled:opacity-50"
              disabled={isLoading}
            >
              Gönder
            </button>
          </form>
        </div>
      )}
    </>
  )
}


