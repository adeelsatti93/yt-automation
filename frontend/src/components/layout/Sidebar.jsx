import { NavLink } from 'react-router-dom'

const navItems = [
  { path: '/', icon: 'bi-speedometer2', label: 'Dashboard' },
  { path: '/episodes', icon: 'bi-film', label: 'Episodes' },
  { path: '/characters', icon: 'bi-people', label: 'Characters' },
  { path: '/topics', icon: 'bi-lightbulb', label: 'Topics' },
  { path: '/pipeline-config', icon: 'bi-diagram-3', label: 'Pipeline Map' },
  { path: '/settings', icon: 'bi-gear', label: 'Settings' },
  { path: '/analytics', icon: 'bi-graph-up', label: 'Analytics' },
]

export default function Sidebar({ collapsed, onToggle }) {
  return (
    <aside className={`app-sidebar ${collapsed ? 'collapsed' : ''}`}>
      <div className="sidebar-header">
        <NavLink to="/" className="sidebar-brand">
          <div className="sidebar-brand-icon">
            <i className="bi bi-play-fill"></i>
          </div>
          {!collapsed && <span className="sidebar-brand-text">CartoonAI</span>}
        </NavLink>
        <button className="sidebar-toggle" onClick={onToggle}>
          <i className={`bi ${collapsed ? 'bi-chevron-right' : 'bi-chevron-left'}`}></i>
        </button>
      </div>
      
      <nav className="sidebar-nav">
        {navItems.map(item => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) => `sidebar-nav-item ${isActive ? 'active' : ''}`}
            end={item.path === '/'}
          >
            <i className={`bi ${item.icon}`}></i>
            <span className="nav-item-label ms-3">{item.label}</span>
          </NavLink>
        ))}
      </nav>
      
      <div className="sidebar-footer">
        {!collapsed && <span className="sidebar-footer-text text-muted">Kids Cartoon Pipeline v1.0</span>}
      </div>
    </aside>
  );
}
