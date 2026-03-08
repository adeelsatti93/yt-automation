import { useApp } from '../../context/AppContext'

export default function TopBar() {
  const { pageTitle, notifications } = useApp()

  return (
    <header className="app-topbar">
      <h2 className="topbar-title">{pageTitle}</h2>
      
      <div className="topbar-actions">
        {notifications.length > 0 && (
          <div className="notification-bell">
            <i className="bi bi-bell"></i>
            <span className="notification-badge">
              {notifications.length}
            </span>
          </div>
        )}
        
        <div className="user-profile">
          <div className="user-profile-avatar">
            <i className="bi bi-person"></i>
          </div>
          <span className="user-profile-name">Administrator</span>
        </div>
      </div>
    </header>
  );
}

