import { createContext, useContext, useReducer, useCallback } from 'react'

// ---------------------------------------------------------------------------
// Action types
// ---------------------------------------------------------------------------
const ActionTypes = {
  TOGGLE_SIDEBAR: 'TOGGLE_SIDEBAR',
  SET_PAGE_TITLE: 'SET_PAGE_TITLE',
  ADD_NOTIFICATION: 'ADD_NOTIFICATION',
  REMOVE_NOTIFICATION: 'REMOVE_NOTIFICATION',
}

// ---------------------------------------------------------------------------
// Initial state
// ---------------------------------------------------------------------------
const initialState = {
  sidebarCollapsed: false,
  pageTitle: 'Dashboard',
  notifications: [],
}

// ---------------------------------------------------------------------------
// Reducer
// ---------------------------------------------------------------------------
let nextNotificationId = 1

function appReducer(state, action) {
  switch (action.type) {
    case ActionTypes.TOGGLE_SIDEBAR:
      return { ...state, sidebarCollapsed: !state.sidebarCollapsed }

    case ActionTypes.SET_PAGE_TITLE:
      return { ...state, pageTitle: action.payload }

    case ActionTypes.ADD_NOTIFICATION:
      return {
        ...state,
        notifications: [
          ...state.notifications,
          {
            id: nextNotificationId++,
            type: action.payload.type || 'info',
            message: action.payload.message,
            timestamp: Date.now(),
          },
        ],
      }

    case ActionTypes.REMOVE_NOTIFICATION:
      return {
        ...state,
        notifications: state.notifications.filter(
          (n) => n.id !== action.payload
        ),
      }

    default:
      return state
  }
}

// ---------------------------------------------------------------------------
// Context
// ---------------------------------------------------------------------------
const AppContext = createContext(undefined)

// ---------------------------------------------------------------------------
// Provider
// ---------------------------------------------------------------------------
export function AppProvider({ children }) {
  const [state, dispatch] = useReducer(appReducer, initialState)

  const toggleSidebar = useCallback(() => {
    dispatch({ type: ActionTypes.TOGGLE_SIDEBAR })
  }, [])

  const setPageTitle = useCallback((title) => {
    dispatch({ type: ActionTypes.SET_PAGE_TITLE, payload: title })
  }, [])

  const addNotification = useCallback(
    (message, type = 'info') => {
      dispatch({
        type: ActionTypes.ADD_NOTIFICATION,
        payload: { message, type },
      })
    },
    []
  )

  const removeNotification = useCallback((id) => {
    dispatch({ type: ActionTypes.REMOVE_NOTIFICATION, payload: id })
  }, [])

  const value = {
    ...state,
    toggleSidebar,
    setPageTitle,
    addNotification,
    removeNotification,
  }

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------
export function useApp() {
  const context = useContext(AppContext)
  if (context === undefined) {
    throw new Error('useApp must be used within an AppProvider')
  }
  return context
}

export default AppContext
