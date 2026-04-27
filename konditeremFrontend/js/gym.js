document.addEventListener("DOMContentLoaded", () => {
    const apiBaseUrl = 'http://localhost:5131';

    const urlParams = new URLSearchParams(window.location.search);
    const gymId = parseInt(urlParams.get('id')) || 1;

    const gymName = document.getElementById('gym-name');
    const gymImage = document.getElementById('gym-image');
    const gymOpening = document.getElementById('gym-opening');
    const gymPhone = document.getElementById('gym-phone');
    const gymEmail = document.getElementById('gym-email');
    const reviewsContainer = document.getElementById('reviews-container');
    const reviewForm = document.getElementById('review-form');

    const favBtn = document.getElementById('fav-btn');
    const favIcon = document.getElementById('fav-icon');
    const carouselItems = document.getElementById('carousel-items');

    function getFavoritesKey() {
        const token = localStorage.getItem("token");
        const email = localStorage.getItem("userEmail");
        const userId = localStorage.getItem("userId");

        if (!token) return null;
        if (email && email !== "undefined") return `gymFavorites_${email}`;
        if (userId && userId !== "undefined") return `gymFavorites_user${userId}`;
        return `gymFavorites_${token.substring(0, 15)}`;
    }

    function getGymOpenText(gym) {
        const openValue = gym.openAt ?? gym.OpenAt ?? null;

        if (!openValue) return "Nincs megadva";
        if (typeof openValue === "string") return openValue;

        if (typeof openValue === "object") {
            return (
                openValue.info ||
                openValue.Info ||
                openValue.name ||
                openValue.Name ||
                openValue.value ||
                openValue.Value ||
                JSON.stringify(openValue)
            );
        }

        return String(openValue);
    }

    function getFavorites() {
        const favKey = getFavoritesKey();
        if (!favKey) return [];
        return JSON.parse(localStorage.getItem(favKey)) || [];
    }

    function saveFavorites(favorites) {
        const favKey = getFavoritesKey();
        if (!favKey) return;
        localStorage.setItem(favKey, JSON.stringify(favorites));
    }

    function updateFavButton() {
        if (!favBtn || !favIcon) return;

        const token = localStorage.getItem("token");
        const favorites = getFavorites();

        if (!token) {
            favIcon.className = 'bi bi-heart fs-4';
            favBtn.classList.remove('btn-danger');
            favBtn.classList.add('btn-outline-danger');
            return;
        }

        if (favorites.includes(gymId)) {
            favIcon.className = 'bi bi-heart-fill fs-4';
            favBtn.classList.remove('btn-outline-danger');
            favBtn.classList.add('btn-danger');
        } else {
            favIcon.className = 'bi bi-heart fs-4';
            favBtn.classList.remove('btn-danger');
            favBtn.classList.add('btn-outline-danger');
        }
    }

    if (favBtn) {
        favBtn.addEventListener('click', () => {
            const token = localStorage.getItem("token");

            if (!token) {
                alert("A kedvencekhez adáshoz be kell jelentkezned!");
                window.location.href = "login.html";
                return;
            }

            let favorites = getFavorites();

            if (favorites.includes(gymId)) {
                favorites = favorites.filter(id => id !== gymId);
            } else {
                favorites.push(gymId);
            }

            saveFavorites(favorites);
            updateFavButton();
        });

        updateFavButton();
    }

    const gymCoordinates = {
        1: { lat: 47.2397, lng: 16.5936 },
        2: { lat: 47.2289, lng: 16.6089 },
        3: { lat: 47.2327, lng: 16.6085 },
        4: { lat: 47.2465, lng: 16.6372 },
        5: { lat: 47.6759, lng: 16.5936 },
        6: { lat: 47.2465, lng: 16.6372 },
        7: { lat: 47.2294, lng: 16.6174 },
        8: { lat: 47.2250, lng: 16.6380 },
        9: { lat: 47.2327, lng: 16.6085 }
    };

    function initMap(lat, lng, name) {
        const mapContainer = document.getElementById('gym-map');
        if (!mapContainer) return;

        const map = L.map('gym-map').setView([lat, lng], 16);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap'
        }).addTo(map);

        L.marker([lat, lng]).addTo(map)
            .bindPopup(`<b>${name}</b><br>Itt található!`)
            .openPopup();
    }

    async function fetchGymDetails() {
        try {
            const response = await fetch(`${apiBaseUrl}/api/Gym/${gymId}`);
            if (!response.ok) throw new Error('Hiba a terem lekérésekor');

            const gym = await response.json();

            const currentName = gym.name || gym.Name || 'Ismeretlen terem';

            if (gymName) gymName.textContent = currentName;
            if (gymOpening) gymOpening.textContent = getGymOpenText(gym);
            if (gymPhone) gymPhone.textContent = gym.phone || gym.Phone || 'Nincs megadva';
            if (gymEmail) gymEmail.textContent = gym.email || gym.Email || 'Nincs megadva';

            let rawImagePath = gym.imagePath || gym.ImagePath || "";

            if (gymImage) {
                if (rawImagePath && rawImagePath.trim() !== '') {
                    const cleanPath = rawImagePath.replace('images/', '').replace('images\\', '');
                    gymImage.src = `${apiBaseUrl}/images/${cleanPath}`;
                }
            }

            if (carouselItems) {
                if (rawImagePath && rawImagePath.trim() !== '') {
                    const cleanPath = rawImagePath.replace('images/', '').replace('images\\', '');
                    const dotIndex = cleanPath.lastIndexOf('.');
                    const baseName = cleanPath.substring(0, dotIndex);
                    const extension = cleanPath.substring(dotIndex);

                    const images = [
                        `${apiBaseUrl}/images/${cleanPath}`,
                        `${apiBaseUrl}/images/${baseName}2${extension}`,
                        `${apiBaseUrl}/images/${baseName}3${extension}`
                    ];

                    carouselItems.innerHTML = '';
                    images.forEach((src, index) => {
                        carouselItems.innerHTML += `
                            <div class="carousel-item ${index === 0 ? 'active' : ''}">
                                <img src="${src}" class="d-block w-100 hero-img"
                                     onerror="this.src='https://via.placeholder.com/800x400?text=Kép+hamarosan...'"
                                     alt="Konditerem kép ${index + 1}">
                            </div>
                        `;
                    });
                } else {
                    carouselItems.innerHTML = `
                        <div class="carousel-item active">
                            <img src="https://via.placeholder.com/800x400.png?text=Adatbázisban+hiányzó+kép" class="d-block w-100 hero-img">
                        </div>
                    `;
                }
            }

            const coords = gymCoordinates[gymId] || { lat: 47.2300, lng: 16.6200 };
            initMap(coords.lat, coords.lng, currentName);

        } catch (error) {
            console.error("Hiba a terem betöltésekor:", error);
            if (gymName) gymName.textContent = "Hiba: Nem fut a C# backend!";
        }
    }

    async function fetchReviews() {
        try {
            const response = await fetch(`${apiBaseUrl}/api/Rating`);
            if (!response.ok) throw new Error('Hálózat hiba');

            const allReviews = await response.json();
            const gymReviews = allReviews.filter(r => r.gymId == gymId || r.GymId == gymId);

            if (reviewsContainer) reviewsContainer.innerHTML = '';

            if (gymReviews.length === 0) {
                if (reviewsContainer) reviewsContainer.innerHTML = '<p class="text-muted">Még nincs értékelés. Légy te az első!</p>';
                return;
            }

            gymReviews.forEach(review => {
                const stars = review.stars || review.Stars || 5;
                const comment = review.message || review.Message || '';
                const nev = review.user?.name || review.User?.Name || `Vevő #${review.userId || review.UserId}`;

                if (reviewsContainer) {
                    reviewsContainer.innerHTML += `
                        <div class="mb-3 border-bottom border-secondary pb-2 bg-dark p-2 rounded shadow-sm">
                            <div class="d-flex justify-content-between align-items-center">
                                <strong style="color: #0ea5e9 !important; font-size: 1.1rem;">${nev}</strong>
                                <span class="star-rating">${'⭐'.repeat(stars)}</span>
                            </div>
                            <p class="mb-1 mt-1" style="padding-left: 5px; color: #ffffff !important;">"${comment}"</p>
                        </div>
                    `;
                }
            });
        } catch (error) {
            console.error("Hiba:", error);
            if (reviewsContainer) reviewsContainer.innerHTML = '<p class="text-danger">Hiba a véleményeknél.</p>';
        }
    }

    fetchGymDetails();
    fetchReviews();

    const buyButton = document.getElementById('buy-ticket-btn');
    if (buyButton) {
        buyButton.addEventListener('click', () => {
            window.location.href = `purchase.html?id=${gymId}`;
        });
    }

    if (reviewForm) {
        reviewForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const commentText = document.getElementById('review-text').value;
            const starsValue = document.getElementById('review-stars').value;
            const token = localStorage.getItem("token");
            const currentUserId = localStorage.getItem("userId");

            if (!token || !currentUserId) {
                alert("A véleményezéshez be kell jelentkezned!");
                window.location.href = "login.html";
                return;
            }

            const ujVelemeny = {
                message: commentText,
                stars: parseInt(starsValue),
                gymId: parseInt(gymId),
                userId: parseInt(currentUserId)
            };

            try {
                const response = await fetch(`${apiBaseUrl}/api/Rating`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify(ujVelemeny)
                });

                if (response.ok) {
                    alert("Vélemény sikeresen elküldve!");
                    reviewForm.reset();
                    fetchReviews();
                } else {
                    alert("Hiba történt a küldés során!");
                }
            } catch (error) {
                console.error(error);
                alert("Nem sikerült elérni a szervert.");
            }
        });
    }
});