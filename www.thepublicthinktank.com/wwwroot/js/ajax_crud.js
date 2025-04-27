function initCreateForm() {
    $('#create-modal').modal('show')

    // Set listener on create form

    $('#create-merch').on('submit', async function (e) {
        e.preventDefault();
        // disable the create button to prevent duplicates
        $("#create-modal *[type=submit]").attr("disabled", true)

        var formData = new FormData(this);

        try {
            const response = await fetch('/merch-editor', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const contentType = response.headers.get('Content-Type');
          
            if (contentType && contentType.includes('application/json')) {

                jsonResponse = await response.json();
              

                // Show success message

                $('#create-feedback').text("Success creating a new product!")

                setTimeout(() => {
                    $("#create-modal").modal('hide')
                    // Add the new product to the table
                    $('table tbody').append(jsonResponse.productRow);
                }, 3000)

            } else {
                $("#create-modal").modal('hide')
                const text = await response.text()
                // replace html, with field validation errors populated
                $("#create").get(0).innerHTML = text;
                $("#create-modal").modal('show')
            }

        } catch (error) {
            console.error('There was a problem with the fetch operation:', error);
        }
    });
}

function initEditForm() {
    $('#edit-modal').modal('show')


    // Set listener on edit form

    $('#edit-merch').on('submit', async function (e) {
        e.preventDefault();

        // disable the create button to prevent duplicates
        $("#edit-modal *[type=submit]").attr("disabled", true)

        // disable the create button to prevent duplicates
        $("#edit-modal button[type=submit]").attr("disabled", true)

        var formData = new FormData(this);

        const productId = e.target.closest('form').getAttribute("data-product-id")

        try {
            const response = await fetch('/merch-editor/edit/' + productId, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const contentType = response.headers.get('Content-Type');

            if (contentType && contentType.includes('application/json')) {

                jsonResponse = await response.json();
                console.log({jsonResponse})


                // Show success message

                $('#edit-feedback').text("Success editing a product!")

                setTimeout(() => {
                    $("#edit-modal").modal('hide')
                    console.log({ jsonResponse })
                    // Add the new product to the table
                    $('table tbody').find(`tr[data-product-id="${jsonResponse.productId}"]`).replaceWith(jsonResponse.productRow);
                }, 3000)

            } else {
                $("#edit-modal").modal('hide')
                const text = await response.text()
                // replace html, with field validation errors populated
                $("#edit").get(0).innerHTML = text;
                $("#edit-modal").modal('show')
            }

        } catch (error) {
            console.error('There was a problem with the fetch operation:', error);
        }
    });
}


function removeMerchItem(productId) {
    if (!confirm('Are you sure you want to delete this item?')) {
        return;
    }

    fetch('/merch-editor/delete/' + productId, {
        method: 'DELETE'
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(jsonResponse => {
        // Show success message
        $('#delete-feedback').text("Success deleting the product!");

        // Remove the product from the table
        $('table tbody').find(`tr[data-product-id="${productId}"]`).remove();
    })
    .catch(error => {
        console.error('There was a problem with the fetch operation:', error);
    });
}