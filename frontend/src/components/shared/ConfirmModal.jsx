export default function ConfirmModal({ show, title = 'Confirm', message, confirmText = 'Confirm', cancelText = 'Cancel', variant = 'danger', onConfirm, onCancel }) {
  if (!show) return null

  return (
    <div className="modal d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} onClick={onCancel}>
      <div className="modal-dialog modal-dialog-centered" onClick={e => e.stopPropagation()}>
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">{title}</h5>
            <button type="button" className="btn-close" onClick={onCancel}></button>
          </div>
          <div className="modal-body">
            <p>{message}</p>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" onClick={onCancel}>{cancelText}</button>
            <button type="button" className={`btn btn-${variant}`} onClick={onConfirm}>{confirmText}</button>
          </div>
        </div>
      </div>
    </div>
  )
}
