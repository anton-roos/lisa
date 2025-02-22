window.initializeMobileNav = () => {
    const navWrapper = document.querySelector('.nav-wrapper');
    const leftArrow = document.querySelector('.left-arrow');
    const rightArrow = document.querySelector('.right-arrow');

    if (!navWrapper || !leftArrow || !rightArrow) {
        return;
    }

    function updateArrowsVisibility() {
        leftArrow.style.display = navWrapper.scrollLeft <= 0 ? 'none' : 'block';
        rightArrow.style.display = navWrapper.scrollLeft + navWrapper.clientWidth >= navWrapper.scrollWidth ? 'none' : 'block';
    }

    leftArrow.addEventListener('click', () => {
        navWrapper.scrollBy({ left: -100, behavior: 'smooth' });
    });

    rightArrow.addEventListener('click', () => {
        navWrapper.scrollBy({ left: 100, behavior: 'smooth' });
    });

    navWrapper.addEventListener('scroll', updateArrowsVisibility);

    updateArrowsVisibility();
};
