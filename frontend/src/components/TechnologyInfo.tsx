import { useState } from 'react'

interface TechnologyInfoProps {
  technologies: string[]
  description?: string
}

export function TechnologyInfo({ technologies, description }: TechnologyInfoProps) {
  const [isHovered, setIsHovered] = useState(false)

  return (
    <div className="relative inline-block">
      <div
        className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-blue-100 hover:bg-blue-200 cursor-help transition-colors"
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
      >
        <svg
          className="w-5 h-5 text-blue-600"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
      </div>

      {isHovered && (
        <div className="absolute z-50 left-0 top-6 w-64 p-3 bg-gray-900 text-white text-sm rounded-lg shadow-xl border border-gray-700">
          <div className="font-semibold mb-2 text-blue-300">Backend Teknolojileri</div>
          {description && (
            <div className="mb-2 text-gray-300 text-xs">{description}</div>
          )}
          <ul className="space-y-1">
            {technologies.map((tech, index) => (
              <li key={index} className="flex items-start">
                <span className="text-green-400 mr-2">•</span>
                <span className="text-gray-200">{tech}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}

