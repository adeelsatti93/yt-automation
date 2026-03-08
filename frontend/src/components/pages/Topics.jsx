import { useEffect, useState } from 'react';
import { useApp } from '../../context/AppContext';
import { useTopics } from '../../hooks/useTopics';
import TopicCard from '../topics/TopicCard';
import TopicForm from '../topics/TopicForm';
import LoadingSpinner from '../shared/LoadingSpinner';
import ErrorAlert from '../shared/ErrorAlert';
import EmptyState from '../shared/EmptyState';
import ConfirmModal from '../shared/ConfirmModal';
import { toast } from 'react-toastify';

export default function Topics() {
  const { setPageTitle } = useApp();
  const {
    topics,
    generatedIdeas,
    loading,
    generating,
    error,
    fetchTopics,
    createTopic,
    updateTopic,
    deleteTopic,
    generateIdeas,
    triggerPipeline,
    clearIdeas,
  } = useTopics();

  const [showForm, setShowForm] = useState(false);
  const [editingTopic, setEditingTopic] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [showIdeas, setShowIdeas] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setPageTitle('Topics');
  }, [setPageTitle]);

  const queuedTopics = topics.filter((t) => !t.isUsed);
  const usedTopics = topics.filter((t) => t.isUsed);

  const handleAdd = () => {
    setEditingTopic(null);
    setShowForm(true);
  };

  const handleEdit = (topic) => {
    setEditingTopic(topic);
    setShowForm(true);
  };

  const handleSubmit = async (data) => {
    setSaving(true);
    try {
      if (editingTopic) {
        await updateTopic(editingTopic.id, data);
      } else {
        await createTopic(data);
      }
      setShowForm(false);
      setEditingTopic(null);
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (deleteTarget) {
      try {
        await deleteTopic(deleteTarget.id);
      } catch {
        // Error handled in hook
      }
      setDeleteTarget(null);
    }
  };

  const handleTrigger = async (topic) => {
    try {
      await triggerPipeline(topic.id);
      toast.success(`Pipeline triggered for "${topic.title}"!`);
      fetchTopics();
    } catch {
      // Error handled in hook
    }
  };

  const handleGenerateIdeas = async () => {
    try {
      await generateIdeas();
      setShowIdeas(true);
    } catch {
      // Error handled in hook
    }
  };

  const handleAddIdea = async (idea) => {
    setSaving(true);
    try {
      await createTopic({ title: idea.title || idea, targetMoral: idea.moral || '' });
      toast.success('Topic added to queue!');
    } finally {
      setSaving(false);
    }
  };

  if (loading && topics.length === 0) {
    return <LoadingSpinner text="Loading topics..." />;
  }

  return (
    <div className="container-fluid">
      {error && <ErrorAlert message={error} onRetry={fetchTopics} />}

      {/* Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <span className="text-muted">
          {queuedTopics.length} queued · {usedTopics.length} used
        </span>
        <div className="d-flex gap-2">
          <button
            className="btn btn-outline-primary"
            onClick={handleGenerateIdeas}
            disabled={generating}
          >
            {generating ? (
              <>
                <span className="spinner-border spinner-border-sm me-1"></span>
                Generating...
              </>
            ) : (
              <>🤖 Generate AI Ideas</>
            )}
          </button>
          <button className="btn btn-primary" onClick={handleAdd}>
            <i className="bi bi-plus-lg me-1"></i>Add Topic
          </button>
        </div>
      </div>

      {/* Queued Topics */}
      <div className="mb-4">
        <h6 className="text-uppercase text-muted mb-3">
          <i className="bi bi-hourglass-split me-2"></i>
          Queued ({queuedTopics.length})
        </h6>
        {queuedTopics.length === 0 ? (
          <EmptyState
            icon="bi-lightbulb"
            title="No topics in queue"
            message="Add topic ideas or let AI generate some for you."
            action={handleAdd}
            actionLabel="Add Topic"
          />
        ) : (
          queuedTopics.map((topic, index) => (
            <div key={topic.id} className="position-relative">
              {index === 0 && (
                <span className="badge bg-primary position-absolute top-0 start-0 translate-middle-y ms-3" style={{ zIndex: 1 }}>
                  Next to produce →
                </span>
              )}
              <TopicCard
                topic={topic}
                onEdit={handleEdit}
                onDelete={setDeleteTarget}
                onTrigger={handleTrigger}
              />
            </div>
          ))
        )}
      </div>

      {/* Used Topics */}
      {usedTopics.length > 0 && (
        <div>
          <h6 className="text-uppercase text-muted mb-3">
            <i className="bi bi-check-circle me-2"></i>
            Used ({usedTopics.length})
          </h6>
          {usedTopics.map((topic) => (
            <TopicCard
              key={topic.id}
              topic={topic}
              onEdit={handleEdit}
              onDelete={setDeleteTarget}
              onTrigger={handleTrigger}
            />
          ))}
        </div>
      )}

      {/* Topic Form Modal */}
      {showForm && (
        <div className="modal d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} onClick={() => setShowForm(false)}>
          <div className="modal-dialog modal-dialog-centered" onClick={(e) => e.stopPropagation()}>
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingTopic ? 'Edit Topic' : 'Add Topic'}
                </h5>
                <button type="button" className="btn-close" onClick={() => setShowForm(false)}></button>
              </div>
              <div className="modal-body">
                <TopicForm
                  topic={editingTopic}
                  onSubmit={handleSubmit}
                  onCancel={() => setShowForm(false)}
                  loading={saving}
                />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* AI Ideas Modal */}
      {showIdeas && (
        <div className="modal d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} onClick={() => { setShowIdeas(false); clearIdeas(); }}>
          <div className="modal-dialog modal-lg modal-dialog-centered" onClick={(e) => e.stopPropagation()}>
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">🤖 AI-Generated Topic Ideas</h5>
                <button type="button" className="btn-close" onClick={() => { setShowIdeas(false); clearIdeas(); }}></button>
              </div>
              <div className="modal-body">
                {generatedIdeas.length === 0 ? (
                  <p className="text-muted text-center py-3">No ideas generated.</p>
                ) : (
                  <div className="list-group">
                    {generatedIdeas.map((idea, index) => {
                      const title = typeof idea === 'string' ? idea : idea.title;
                      const moral = typeof idea === 'object' ? idea.moral : '';
                      return (
                        <div key={index} className="list-group-item d-flex justify-content-between align-items-center">
                          <div>
                            <strong>{title}</strong>
                            {moral && <small className="text-muted d-block">{moral}</small>}
                          </div>
                          <button
                            className="btn btn-sm btn-outline-primary"
                            onClick={() => handleAddIdea(idea)}
                            disabled={saving}
                          >
                            <i className="bi bi-plus-lg me-1"></i>Add
                          </button>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation */}
      <ConfirmModal
        show={!!deleteTarget}
        title="Delete Topic"
        message={`Are you sure you want to delete "${deleteTarget?.title}"?`}
        confirmText="Delete"
        variant="danger"
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
