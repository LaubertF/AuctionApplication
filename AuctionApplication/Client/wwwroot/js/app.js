function closeModal(elementId) {
    var myModalEl = document.getElementById(elementId);
    var modal = bootstrap.Modal.getInstance(myModalEl);
    if (modal) {
        modal.hide();
        
        myModalEl.addEventListener('hidden.bs.modal', function () {
            document.querySelector('.modal-backdrop').remove();
            document.body.style.overflow = 'auto';
            document.body.style.paddingRight = '0px';
        });
    }
}
function watchInput(elementId, minValue) {
    var element = document.getElementById(elementId);
    if (element) {
        element.addEventListener("change", function(e) {
            if (e.target.value > minValue) {
                element.classList.remove('is-invalid');
                element.classList.add('is-valid');
            } else {
                element.classList.remove('is-valid');
                element.classList.add('is-invalid');
            }
        });
    }
}
function makeInputInvalid(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.classList.remove('valid');
        element.classList.add('is-invalid');
    }
}