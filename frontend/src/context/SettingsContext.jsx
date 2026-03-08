import {
  createContext,
  useContext,
  useReducer,
  useEffect,
  useCallback,
} from 'react'
import { settingsApi } from '../api/settingsApi'

// ---------------------------------------------------------------------------
// Action types
// ---------------------------------------------------------------------------
const ActionTypes = {
  LOAD_START: 'LOAD_START',
  LOAD_SUCCESS: 'LOAD_SUCCESS',
  LOAD_ERROR: 'LOAD_ERROR',
  UPDATE_SETTING: 'UPDATE_SETTING',
  BATCH_UPDATE: 'BATCH_UPDATE',
  CONNECTION_TEST_START: 'CONNECTION_TEST_START',
  CONNECTION_TEST_RESULT: 'CONNECTION_TEST_RESULT',
  CONNECTION_TEST_ERROR: 'CONNECTION_TEST_ERROR',
}

// ---------------------------------------------------------------------------
// Setting categories
// ---------------------------------------------------------------------------
const SETTING_CATEGORIES = ['api_keys', 'youtube', 'pipeline', 'storage']

function groupByCategory(settings) {
  const grouped = {}
  for (const category of SETTING_CATEGORIES) {
    grouped[category] = {}
  }
  // Place any uncategorised settings under their own key
  if (Array.isArray(settings)) {
    for (const setting of settings) {
      const cat = setting.category || 'pipeline'
      if (!grouped[cat]) grouped[cat] = {}
      grouped[cat][setting.key] = setting
    }
  } else if (settings && typeof settings === 'object') {
    for (const [key, value] of Object.entries(settings)) {
      const cat = value?.category || 'pipeline'
      if (!grouped[cat]) grouped[cat] = {}
      grouped[cat][key] = value
    }
  }
  return grouped
}

// ---------------------------------------------------------------------------
// Initial state
// ---------------------------------------------------------------------------
const initialState = {
  settings: {},
  grouped: groupByCategory([]),
  loading: false,
  error: null,
  connectionTests: {},
}

// ---------------------------------------------------------------------------
// Reducer
// ---------------------------------------------------------------------------
function settingsReducer(state, action) {
  switch (action.type) {
    case ActionTypes.LOAD_START:
      return { ...state, loading: true, error: null }

    case ActionTypes.LOAD_SUCCESS: {
      const settings = action.payload
      return {
        ...state,
        loading: false,
        settings,
        grouped: groupByCategory(settings),
        error: null,
      }
    }

    case ActionTypes.LOAD_ERROR:
      return { ...state, loading: false, error: action.payload }

    case ActionTypes.UPDATE_SETTING: {
      const { key, value } = action.payload
      const updated = Array.isArray(state.settings)
        ? state.settings.map((s) => (s.key === key ? { ...s, ...value } : s))
        : { ...state.settings, [key]: value }
      return {
        ...state,
        settings: updated,
        grouped: groupByCategory(updated),
      }
    }

    case ActionTypes.BATCH_UPDATE: {
      const changes = action.payload
      let updated
      if (Array.isArray(state.settings)) {
        updated = state.settings.map((s) =>
          changes[s.key] ? { ...s, ...changes[s.key] } : s
        )
      } else {
        updated = { ...state.settings, ...changes }
      }
      return {
        ...state,
        settings: updated,
        grouped: groupByCategory(updated),
      }
    }

    case ActionTypes.CONNECTION_TEST_START:
      return {
        ...state,
        connectionTests: {
          ...state.connectionTests,
          [action.payload]: { testing: true, result: null, error: null },
        },
      }

    case ActionTypes.CONNECTION_TEST_RESULT:
      return {
        ...state,
        connectionTests: {
          ...state.connectionTests,
          [action.payload.service]: {
            testing: false,
            result: action.payload.result,
            error: null,
          },
        },
      }

    case ActionTypes.CONNECTION_TEST_ERROR:
      return {
        ...state,
        connectionTests: {
          ...state.connectionTests,
          [action.payload.service]: {
            testing: false,
            result: null,
            error: action.payload.error,
          },
        },
      }

    default:
      return state
  }
}

// ---------------------------------------------------------------------------
// Context
// ---------------------------------------------------------------------------
const SettingsContext = createContext(undefined)

// ---------------------------------------------------------------------------
// Provider
// ---------------------------------------------------------------------------
export function SettingsProvider({ children }) {
  const [state, dispatch] = useReducer(settingsReducer, initialState)

  // Load settings on mount
  const loadSettings = useCallback(async () => {
    dispatch({ type: ActionTypes.LOAD_START })
    try {
      const { data } = await settingsApi.getAll()
      dispatch({ type: ActionTypes.LOAD_SUCCESS, payload: data })
    } catch (err) {
      dispatch({
        type: ActionTypes.LOAD_ERROR,
        payload: err.response?.data?.message || err.message,
      })
    }
  }, [])

  useEffect(() => {
    loadSettings()
  }, [loadSettings])

  const updateSetting = useCallback(async (key, value) => {
    try {
      await settingsApi.update(key, value)
      dispatch({
        type: ActionTypes.UPDATE_SETTING,
        payload: { key, value },
      })
    } catch (err) {
      dispatch({
        type: ActionTypes.LOAD_ERROR,
        payload: err.response?.data?.message || err.message,
      })
      throw err
    }
  }, [])

  const batchUpdateSettings = useCallback(async (changes) => {
    try {
      await settingsApi.batchUpdate(changes)
      dispatch({ type: ActionTypes.BATCH_UPDATE, payload: changes })
    } catch (err) {
      dispatch({
        type: ActionTypes.LOAD_ERROR,
        payload: err.response?.data?.message || err.message,
      })
      throw err
    }
  }, [])

  const testConnection = useCallback(async (service) => {
    dispatch({ type: ActionTypes.CONNECTION_TEST_START, payload: service })
    try {
      const { data } = await settingsApi.testConnection(service)
      dispatch({
        type: ActionTypes.CONNECTION_TEST_RESULT,
        payload: { service, result: data },
      })
      return data
    } catch (err) {
      const errorMsg = err.response?.data?.message || err.message
      dispatch({
        type: ActionTypes.CONNECTION_TEST_ERROR,
        payload: { service, error: errorMsg },
      })
      throw err
    }
  }, [])

  const value = {
    settings: state.settings,
    grouped: state.grouped,
    loading: state.loading,
    error: state.error,
    connectionTests: state.connectionTests,
    loadSettings,
    updateSetting,
    batchUpdateSettings,
    testConnection,
  }

  return (
    <SettingsContext.Provider value={value}>
      {children}
    </SettingsContext.Provider>
  )
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------
export function useSettings() {
  const context = useContext(SettingsContext)
  if (context === undefined) {
    throw new Error('useSettings must be used within a SettingsProvider')
  }
  return context
}

export default SettingsContext
