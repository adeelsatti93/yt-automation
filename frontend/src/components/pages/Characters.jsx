import { useEffect, useState } from 'react';
import { useApp } from '../../context/AppContext';
import { useCharacters } from '../../hooks/useCharacters';
import CharacterCard from '../characters/CharacterCard';
import CharacterForm from '../characters/CharacterForm';
import LoadingSpinner from '../shared/LoadingSpinner';
import ErrorAlert from '../shared/ErrorAlert';
import EmptyState from '../shared/EmptyState';
import ConfirmModal from '../shared/ConfirmModal';

export default function Characters() {
  const { setPageTitle } = useApp();
  const {
    characters,
    voices,
    loading,
    error,
    fetchCharacters,
    createCharacter,
    updateCharacter,
    deleteCharacter,
    fetchVoices,
    testVoice,
  } = useCharacters();

  const [showForm, setShowForm] = useState(false);
  const [editingCharacter, setEditingCharacter] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setPageTitle('Characters');
  }, [setPageTitle]);

  const handleAdd = () => {
    setEditingCharacter(null);
    setShowForm(true);
    if (voices.length === 0) fetchVoices();
  };

  const handleEdit = (character) => {
    setEditingCharacter(character);
    setShowForm(true);
    if (voices.length === 0) fetchVoices();
  };

  const handleSubmit = async (data) => {
    setSaving(true);
    try {
      if (editingCharacter) {
        await updateCharacter(editingCharacter.id, data);
      } else {
        await createCharacter(data);
      }
      setShowForm(false);
      setEditingCharacter(null);
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (deleteTarget) {
      try {
        await deleteCharacter(deleteTarget.id);
      } catch {
        // Error handled in hook
      }
      setDeleteTarget(null);
    }
  };

  const handleTestVoice = async (character) => {
    if (character.voiceId) {
      await testVoice(character.id, `Hi! I'm ${character.name}!`);
    }
  };

  if (loading && characters.length === 0) {
    return <LoadingSpinner text="Loading characters..." />;
  }

  return (
    <div className="container-fluid">
      {error && <ErrorAlert message={error} onRetry={fetchCharacters} />}

      {/* Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <span className="text-muted">
          {characters.length} character{characters.length !== 1 ? 's' : ''}
        </span>
        <button className="btn btn-primary" onClick={handleAdd}>
          <i className="bi bi-plus-lg me-1"></i>Add Character
        </button>
      </div>

      {/* Character Grid */}
      {characters.length === 0 ? (
        <EmptyState
          icon="bi-people"
          title="No characters yet"
          message="Add your first character to get started with episode generation."
          action={handleAdd}
          actionLabel="Add Character"
        />
      ) : (
        <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-3">
          {characters.map((character) => (
            <div className="col" key={character.id}>
              <CharacterCard
                character={character}
                onEdit={handleEdit}
                onDelete={setDeleteTarget}
                onTestVoice={handleTestVoice}
              />
            </div>
          ))}
        </div>
      )}

      {/* Character Form Modal */}
      {showForm && (
        <div className="modal d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} onClick={() => setShowForm(false)}>
          <div className="modal-dialog modal-lg modal-dialog-centered" onClick={(e) => e.stopPropagation()}>
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingCharacter ? 'Edit Character' : 'Add Character'}
                </h5>
                <button type="button" className="btn-close" onClick={() => setShowForm(false)}></button>
              </div>
              <div className="modal-body">
                <CharacterForm
                  character={editingCharacter}
                  voices={voices}
                  onSubmit={handleSubmit}
                  onCancel={() => setShowForm(false)}
                  loading={saving}
                />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation */}
      <ConfirmModal
        show={!!deleteTarget}
        title="Delete Character"
        message={`Are you sure you want to delete "${deleteTarget?.name}"? This will not affect existing episodes.`}
        confirmText="Delete"
        variant="danger"
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
