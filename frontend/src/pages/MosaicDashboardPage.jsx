import React from 'react'
import FilterButton from '../components/DropdownFilter'
import Datepicker from '../components/Datepicker'
import Banner from '../partials/Banner'
import { MosaicDashboardProvider, useMosaicDashboard } from '../context/MosaicDashboardContext'
import DashboardCard01 from '../partials/dashboard/DashboardCard01'
import DashboardCard02 from '../partials/dashboard/DashboardCard02'
import DashboardCard03 from '../partials/dashboard/DashboardCard03'
import DashboardCard04 from '../partials/dashboard/DashboardCard04'
import DashboardCard05 from '../partials/dashboard/DashboardCard05'
import DashboardCard06 from '../partials/dashboard/DashboardCard06'
import DashboardCard07 from '../partials/dashboard/DashboardCard07'
import DashboardCard08 from '../partials/dashboard/DashboardCard08'
import DashboardCard09 from '../partials/dashboard/DashboardCard09'
import DashboardCard10 from '../partials/dashboard/DashboardCard10'
import DashboardCard11 from '../partials/dashboard/DashboardCard11'
import DashboardCard12 from '../partials/dashboard/DashboardCard12'
import DashboardCard13 from '../partials/dashboard/DashboardCard13'

function MosaicDashboardContent() {
  const { isLoading, isError, error } = useMosaicDashboard()

  return (
    <>
      <div className="sm:flex sm:justify-between sm:items-center mb-8">
        <div className="mb-4 sm:mb-0">
          <h1 className="text-2xl md:text-3xl text-gray-800 dark:text-gray-100 font-bold">Dashboard</h1>
        </div>
        <div className="grid grid-flow-col sm:auto-cols-max justify-start sm:justify-end gap-2">
          <FilterButton align="right" />
          <Datepicker />
          <button
            type="button"
            className="btn bg-gray-900 text-gray-100 hover:bg-gray-800 dark:bg-gray-100 dark:text-gray-800 dark:hover:bg-white"
          >
            <svg className="fill-current shrink-0 max-xs:hidden" width="16" height="16" viewBox="0 0 16 16">
              <path d="M15 7H9V1c0-.6-.4-1-1-1S7 .4 7 1v6H1c-.6 0-1 .4-1 1s.4 1 1 1h6v6c0 .6.4 1 1 1s1-.4 1-1V9h6c.6 0 1-.4 1-1s-.4-1-1-1z" />
            </svg>
            <span className="max-xs:sr-only">Add View</span>
          </button>
        </div>
      </div>

      {isLoading && (
        <div className="flex justify-center items-center min-h-[320px]">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-violet-500" />
        </div>
      )}
      {isError && (
        <div className="rounded-xl bg-red-500/10 text-red-600 dark:text-red-400 p-6 text-center">
          {error?.message ?? 'Dashboard verisi yüklenemedi.'}
        </div>
      )}
      {!isLoading && !isError && (
        <div className="grid grid-cols-12 gap-6">
          <DashboardCard01 />
          <DashboardCard02 />
          <DashboardCard03 />
          <DashboardCard04 />
          <DashboardCard05 />
          <DashboardCard06 />
          <DashboardCard07 />
          <DashboardCard08 />
          <DashboardCard09 />
          <DashboardCard10 />
          <DashboardCard11 />
          <DashboardCard12 />
          <DashboardCard13 />
        </div>
      )}
      <Banner />
    </>
  )
}

export default function MosaicDashboardPage() {
  return (
    <MosaicDashboardProvider>
      <MosaicDashboardContent />
    </MosaicDashboardProvider>
  )
}
