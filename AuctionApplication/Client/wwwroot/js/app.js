function closeModal(elementId) {
    var myModalEl = document.getElementById(elementId);
    var modal = bootstrap.Modal.getInstance(myModalEl)
    if (modal) {
        modal.hide();
        var modalBackdrop = document.querySelector('.modal-backdrop');
        if (modalBackdrop) {
            modalBackdrop.remove();
        }
    }
}

window.asyncRemoveBackdrop = async function() {
    await removeBackdrop();
};

function removeBackdrop() {
    console.log("removeBackdrop");
    var modalBackdrop = document.querySelector('.modal-backdrop');
    if (modalBackdrop) {
        console.log("odstranujem");
        modalBackdrop.remove();
    } else {
        console.log("nenasla som");
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