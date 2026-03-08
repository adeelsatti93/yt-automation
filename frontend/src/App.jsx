import { Routes, Route } from 'react-router-dom'
import Layout from './components/layout/Layout'
import Dashboard from './components/pages/Dashboard'
import Episodes from './components/pages/Episodes'
import EpisodeDetail from './components/pages/EpisodeDetail'
import Characters from './components/pages/Characters'
import Topics from './components/pages/Topics'
import Settings from './components/pages/Settings'
import Analytics from './components/pages/Analytics'

import PipelineMap from './components/pages/PipelineMap'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Dashboard />} />
        <Route path="/episodes" element={<Episodes />} />
        <Route path="/episodes/:id" element={<EpisodeDetail />} />
        <Route path="/characters" element={<Characters />} />
        <Route path="/topics" element={<Topics />} />
        <Route path="/settings" element={<Settings />} />
        <Route path="/analytics" element={<Analytics />} />
        <Route path="/pipeline-config" element={<PipelineMap />} />
      </Route>
    </Routes>
  )
}
