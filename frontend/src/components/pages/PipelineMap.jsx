import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../../context/AppContext';

const SYSTEM_NODES = [
  { 
    id: 'settings', 
    label: 'Config Engine', 
    icon: 'bi-gear-fill', 
    description: 'System Personality & Global API Config',
    x: 200, y: 300,
    route: '/settings',
    actions: ['API Connectivity', 'Prompts']
  },
  { 
    id: 'characters', 
    label: 'Persona Factory', 
    icon: 'bi-people-fill', 
    description: 'Voice & Visual Identity Design Lab',
    x: 500, y: 150,
    route: '/characters',
    actions: ['Voice Test', 'Design']
  },
  { 
    id: 'topics', 
    label: 'Idea Lab', 
    icon: 'bi-lightbulb-fill', 
    description: 'Strategic AI Topic Generation Hub',
    x: 500, y: 450,
    route: '/topics',
    actions: ['Mass Gen', 'Curation']
  },
  { 
    id: 'episodes', 
    label: 'Media Forge', 
    icon: 'bi-film', 
    description: 'End-to-End Production & Distribution',
    x: 800, y: 300,
    route: '/episodes',
    actions: ['Studio', 'Export']
  }
];

export default function PipelineMap() {
  const navigate = useNavigate();
  const { setPageTitle } = useApp();
  const [activeNode, setActiveNode] = useState(null);
  
  useEffect(() => {
    setPageTitle('System Map');
  }, [setPageTitle]);

  const drawConnection = (start, end) => {
    const dx = end.x - start.x;
    const dy = end.y - start.y;
    const midX = start.x + dx / 2;
    return `M ${start.x} ${start.y} C ${midX} ${start.y}, ${midX} ${end.y}, ${end.x} ${end.y}`;
  };

  return (
    <div className="pipeline-map-wrapper">
      {/* Board Canvas */}
      <svg className="flow-path">
        {/* Connection Lines */}
        <path d={drawConnection(SYSTEM_NODES[0], SYSTEM_NODES[1])} />
        <path d={drawConnection(SYSTEM_NODES[0], SYSTEM_NODES[2])} />
        <path d={drawConnection(SYSTEM_NODES[1], SYSTEM_NODES[3])} />
        <path d={drawConnection(SYSTEM_NODES[2], SYSTEM_NODES[3])} />
      </svg>

      {/* Nodes / Board Cards */}
      {SYSTEM_NODES.map((node) => (
        <div 
          key={node.id}
          className={`system-node-card ${activeNode === node.id ? 'active-node' : ''}`}
          style={{ 
            position: 'absolute',
            left: `${node.x}px`,
            top: `${node.y}px`,
            transform: 'translate(-50%, -50%)' 
          }}
          onClick={() => navigate(node.route)}
          onMouseEnter={() => setActiveNode(node.id)}
          onMouseLeave={() => setActiveNode(null)}
        >
          <div className="node-icon-wrapper">
            <i className={`bi ${node.icon}`}></i>
          </div>
          <div className="node-title">{node.label}</div>
          <div className="node-desc">{node.description}</div>
          
          {/* Action Tags */}
          <div className="d-flex flex-wrap gap-1 justify-content-center">
            {node.actions.map(action => (
              <span key={action} className="node-tag">
                {action}
              </span>
            ))}
          </div>
        </div>
      ))}
      
      {/* Strategic Legend */}
      <div className="position-absolute bottom-0 start-0 p-5">
        <div className="bg-white p-3 rounded-lg shadow-sm border border-slate-200">
          <h6 className="smaller text-slate-400 text-uppercase fw-extrabold mb-3 ls-wide">Strategic Legend</h6>
          <div className="d-flex flex-column gap-2">
            <div className="d-flex align-items-center gap-2">
              <span className="rounded-circle bg-indigo-500" style={{ width: '8px', height: '8px', backgroundColor: '#4f46e5' }}></span>
              <span className="smaller text-slate-600 fw-bold">Active Operational Path</span>
            </div>
            <div className="d-flex align-items-center gap-2">
              <span className="rounded-circle" style={{ width: '8px', height: '8px', border: '2px solid #e2e8f0' }}></span>
              <span className="smaller text-slate-500">Standby System Component</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
