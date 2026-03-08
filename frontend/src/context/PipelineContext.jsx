import {
  createContext,
  useContext,
  useReducer,
  useEffect,
  useCallback,
  useRef,
} from 'react'
import { pipelineApi } from '../api/pipelineApi'

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------
const AUTO_REFRESH_INTERVAL = 5000 // 5 seconds

// ---------------------------------------------------------------------------
// Action types
// ---------------------------------------------------------------------------
const ActionTypes = {
  SET_STATUS: 'SET_STATUS',
  SET_LOGS: 'SET_LOGS',
  SET_SCHEDULE: 'SET_SCHEDULE',
  SET_LOADING: 'SET_LOADING',
  SET_ERROR: 'SET_ERROR',
  TRIGGER_START: 'TRIGGER_START',
  TRIGGER_SUCCESS: 'TRIGGER_SUCCESS',
  TRIGGER_ERROR: 'TRIGGER_ERROR',
}

// ---------------------------------------------------------------------------
// Initial state
// ---------------------------------------------------------------------------
const initialState = {
  status: null,
  logs: [],
  schedule: null,
  loading: false,
  error: null,
  triggering: false,
}

// ---------------------------------------------------------------------------
// Reducer
// ---------------------------------------------------------------------------
function pipelineReducer(state, action) {
  switch (action.type) {
    case ActionTypes.SET_STATUS:
      return { ...state, status: action.payload, error: null }

    case ActionTypes.SET_LOGS:
      return { ...state, logs: action.payload }

    case ActionTypes.SET_SCHEDULE:
      return { ...state, schedule: action.payload }

    case ActionTypes.SET_LOADING:
      return { ...state, loading: action.payload }

    case ActionTypes.SET_ERROR:
      return { ...state, error: action.payload, loading: false }

    case ActionTypes.TRIGGER_START:
      return { ...state, triggering: true, error: null }

    case ActionTypes.TRIGGER_SUCCESS:
      return { ...state, triggering: false, status: action.payload }

    case ActionTypes.TRIGGER_ERROR:
      return { ...state, triggering: false, error: action.payload }

    default:
      return state
  }
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
function isRunning(status) {
  return status?.state === 'running' || status?.running === true
}

// ---------------------------------------------------------------------------
// Context
// ---------------------------------------------------------------------------
const PipelineContext = createContext(undefined)

// ---------------------------------------------------------------------------
// Provider
// ---------------------------------------------------------------------------
export function PipelineProvider({ children }) {
  const [state, dispatch] = useReducer(pipelineReducer, initialState)
  const intervalRef = useRef(null)

  // -- Fetch status --------------------------------------------------------
  const refreshStatus = useCallback(async () => {
    try {
      const { data } = await pipelineApi.getStatus()
      dispatch({ type: ActionTypes.SET_STATUS, payload: data })
      return data
    } catch (err) {
      dispatch({
        type: ActionTypes.SET_ERROR,
        payload: err.response?.data?.message || err.message,
      })
      return null
    }
  }, [])

  // -- Load logs -----------------------------------------------------------
  const loadLogs = useCallback(async (params = {}) => {
    dispatch({ type: ActionTypes.SET_LOADING, payload: true })
    try {
      const { data } = await pipelineApi.getLogs(params)
      dispatch({ type: ActionTypes.SET_LOGS, payload: data })
      return data
    } catch (err) {
      dispatch({
        type: ActionTypes.SET_ERROR,
        payload: err.response?.data?.message || err.message,
      })
      return []
    } finally {
      dispatch({ type: ActionTypes.SET_LOADING, payload: false })
    }
  }, [])

  // -- Load schedule -------------------------------------------------------
  const loadSchedule = useCallback(async () => {
    try {
      const { data } = await pipelineApi.getSchedule()
      dispatch({ type: ActionTypes.SET_SCHEDULE, payload: data })
      return data
    } catch (err) {
      dispatch({
        type: ActionTypes.SET_ERROR,
        payload: err.response?.data?.message || err.message,
      })
      return null
    }
  }, [])

  // -- Trigger pipeline ----------------------------------------------------
  const triggerPipeline = useCallback(
    async (params = {}) => {
      dispatch({ type: ActionTypes.TRIGGER_START })
      try {
        const { data } = await pipelineApi.trigger(params)
        dispatch({ type: ActionTypes.TRIGGER_SUCCESS, payload: data })
        // Kick off a status refresh so the UI updates immediately
        await refreshStatus()
        return data
      } catch (err) {
        const msg = err.response?.data?.message || err.message
        dispatch({ type: ActionTypes.TRIGGER_ERROR, payload: msg })
        throw err
      }
    },
    [refreshStatus]
  )

  // -- Pause pipeline ------------------------------------------------------
  const pausePipeline = useCallback(async () => {
    try {
      await pipelineApi.pause()
      await refreshStatus()
    } catch (err) {
      dispatch({
        type: ActionTypes.SET_ERROR,
        payload: err.response?.data?.message || err.message,
      })
      throw err
    }
  }, [refreshStatus])

  // -- Resume pipeline -----------------------------------------------------
  const resumePipeline = useCallback(async () => {
    try {
      await pipelineApi.resume()
      await refreshStatus()
    } catch (err) {
      dispatch({
        type: ActionTypes.SET_ERROR,
        payload: err.response?.data?.message || err.message,
      })
      throw err
    }
  }, [refreshStatus])

  // -- Auto-refresh when pipeline is running -------------------------------
  useEffect(() => {
    // Clear any existing interval
    if (intervalRef.current) {
      clearInterval(intervalRef.current)
      intervalRef.current = null
    }

    if (isRunning(state.status)) {
      intervalRef.current = setInterval(refreshStatus, AUTO_REFRESH_INTERVAL)
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current)
        intervalRef.current = null
      }
    }
  }, [state.status, refreshStatus])

  // -- Fetch initial status and schedule on mount --------------------------
  useEffect(() => {
    refreshStatus()
    loadSchedule()
  }, [refreshStatus, loadSchedule])

  const value = {
    status: state.status,
    logs: state.logs,
    schedule: state.schedule,
    loading: state.loading,
    error: state.error,
    triggering: state.triggering,
    isRunning: isRunning(state.status),
    triggerPipeline,
    pausePipeline,
    resumePipeline,
    refreshStatus,
    loadLogs,
    loadSchedule,
  }

  return (
    <PipelineContext.Provider value={value}>
      {children}
    </PipelineContext.Provider>
  )
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------
export function usePipeline() {
  const context = useContext(PipelineContext)
  if (context === undefined) {
    throw new Error('usePipeline must be used within a PipelineProvider')
  }
  return context
}

export default PipelineContext
