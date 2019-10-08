let loading = false;

function onCommit() {
  if (loading) {
    return;
  }
  loading = true;
  $.ajax('/xxx').done(() => {
    loading = false;
  });
}
