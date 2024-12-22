const banner = document.querySelector('.banner');

function activate(e) {
    const items = document.querySelectorAll('.item');
    e.target.matches('.next') && banner.append(items[0])
    e.target.matches('.prev') && banner.prepend(items[items.length - 1]);
}

document.addEventListener('click', activate, false);