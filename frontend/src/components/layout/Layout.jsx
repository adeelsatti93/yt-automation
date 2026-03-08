import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import Sidebar from './Sidebar'
import TopBar from './TopBar'

export default function Layout() {
  const [collapsed, setCollapsed] = useState(false)

  return (
    <div className="app-layout">
      <Sidebar collapsed={collapsed} onToggle={() => setCollapsed(c => !c)} />
      <div className="main-container">
        <TopBar />
        <main className="content-wrapper">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
