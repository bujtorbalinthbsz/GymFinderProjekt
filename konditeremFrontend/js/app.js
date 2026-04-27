// --- BEÁLLÍTÁSOK ---
const SERVER_URL = "http://localhost:5131";
const API_URL = `${SERVER_URL}/api`;

let allGyms = [];

// --- SEGÉDFÜGGVÉNYEK ---
function getGymName(gym) {
    return gym.name || gym.Name || "Nincs név";
}

function getGymPhone(gym) {
    return gym.phone || gym.Phone || "Nincs adat";
}

function getGymEmail(gym) {
    return gym.email || gym.Email || "Nincs adat";
}

function getGymId(gym) {
    return gym.id || gym.Id || 0;
}

function getGymImagePath(gym) {
    return gym.imagePath || gym.ImagePath || "";
}

function getGymOpenText(gym) {
    const openValue = gym.openAt ?? gym.OpenAt ?? null;

    if (!openValue) return "Nincs adat";

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

// --- BETÖLTÉSKOR FUTÓ FÜGGVÉNYEK (ROUTER) ---
document.addEventListener("DOMContentLoaded", () => {
    updateAuthUI();

    const path = window.location.pathname;

    if (path.includes("index.html") || path === "/" || path === "") {
        loadGyms();
        const searchInput = document.getElementById("gymSearch");
        if (searchInput) {
            searchInput.addEventListener("input", (e) => renderGyms(e.target.value));
        }
    } else if (path.includes("gym.html")) {
        loadOneGymDetails();
    } else if (path.includes("purchase.html")) {
        handlePurchase();
    } else if (path.includes("profil.html")) {
        loadMyTickets();
        loadMyFavorites();
    }
    if (window.location.pathname.includes("kapcsolat.html")) {
    initContactMap();
}
});

// --- 1. KONDITERMEK BETÖLTÉSE (FŐOLDAL) ---
async function loadGyms() {
    const container = document.getElementById("gym-container");
    const spinner = document.getElementById("loading-spinner");
    if (!container) return;

    if (spinner) spinner.style.display = "block";

    try {
        const response = await fetch(`${API_URL}/Gym`);
        if (!response.ok) throw new Error("Hiba a termek lekérésekor");

        allGyms = await response.json();
        renderGyms();
    } catch (error) {
        container.innerHTML = "<p class='text-danger'>Hiba az adatok betöltésekor!</p>";
        console.error("API hiba:", error);
    } finally {
        if (spinner) spinner.style.display = "none";
    }
}

// --- 2. KÁRTYÁK MEGJELENÍTÉSE ÉS KERESÉS ---
function renderGyms(filter = "") {
    const container = document.getElementById("gym-container");
    if (!container) return;

    container.innerHTML = "";

    const filteredGyms = allGyms.filter(gym => {
        const name = getGymName(gym).toLowerCase();

        let city = "Ismeretlen";
        if (name.includes("best fitness")) {
            city = "Sopron";
        } else {
            city = "Szombathely";
        }

        const searchTerm = filter.toLowerCase();
        return name.includes(searchTerm) || city.toLowerCase().includes(searchTerm);
    });

    if (filteredGyms.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center p-5">
                <div class="card bg-dark border-secondary p-4 m-auto" style="max-width: 500px;">
                    <h3 style="color: #0ea5e9 !important; font-weight: bold;">Nincs találat vagy üres az adatbázis</h3>
                    <p style="color: #ffffff !important;">Sajnos nem találtunk "${filter}" kifejezésre illő termet. (Vagy még nincs felvive adat!)</p>
                    <button class="btn btn-outline-primary mt-3" onclick="document.getElementById('gymSearch').value=''; renderGyms('');">Összes terem mutatása</button>
                </div>
            </div>
        `;
        return;
    }

    filteredGyms.forEach(gym => {
        const name = getGymName(gym);
        const city = name.toLowerCase().includes("best fitness") ? "Sopron" : "Szombathely";
        const open = getGymOpenText(gym);
        const phone = getGymPhone(gym);
        const id = getGymId(gym);

        let rawPath = getGymImagePath(gym);
        const cleanPath = rawPath.replace("images/", "").replace("images\\", "");
        const imageSrc = cleanPath ? `${SERVER_URL}/images/${cleanPath}` : "";

        container.innerHTML += `
            <div class="col-12 col-md-6 col-lg-4 mb-4">
                <div class="gym-card">
                    <img src="${imageSrc}" onerror="this.onerror=null; this.src=''; this.alt='Nincs kép';">
                    <h3>${name}</h3>
                    <p style="color: #0ea5e9; font-weight: bold;">Város: ${city}</p>
                    <p>Nyitvatartás: ${open}</p>
                    <p>Telefonszám: ${phone}</p>
                    <a href="gym.html?id=${id}" class="btn full">Részletek</a>
                </div>
            </div>
        `;
    });
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

function initGymMap(lat, lng, name) {
    const mapContainer = document.getElementById("gym-map");
    if (!mapContainer) return;

    if (mapContainer._leaflet_id) {
        mapContainer._leaflet_id = null;
        mapContainer.innerHTML = "";
    }

    const map = L.map("gym-map").setView([lat, lng], 16);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap"
    }).addTo(map);

    L.marker([lat, lng]).addTo(map)
        .bindPopup(`<b>${name}</b><br>Itt található!`)
        .openPopup();
}

// --- 3. RÉSZLETEK BETÖLTÉSE (GYM.HTML) ---
async function loadOneGymDetails() {
    const urlParams = new URLSearchParams(window.location.search);
    const id = urlParams.get("id");
    if (!id) return;

    try {
        const response = await fetch(`${API_URL}/Gym/${id}`);
        if (!response.ok) throw new Error("Hiba a terem részleteinek lekérésekor");

        const gym = await response.json();

        const nameElement = document.getElementById("gym-name");
        const openingElement = document.getElementById("gym-opening");
        const phoneElement = document.getElementById("gym-phone");
        const emailElement = document.getElementById("gym-email");
        const carouselItems = document.getElementById("carousel-items");
        const reviewsContainer = document.getElementById("reviews-container");
        const reviewForm = document.getElementById("review-form");

        if (nameElement) nameElement.innerText = getGymName(gym);
        if (openingElement) openingElement.innerText = getGymOpenText(gym);
        if (phoneElement) phoneElement.innerText = getGymPhone(gym);
        if (emailElement) emailElement.innerText = getGymEmail(gym);

        const gymNumericId = parseInt(id);
        const coords = gymCoordinates[gymNumericId] || { lat: 47.2300, lng: 16.6200 };
        initGymMap(coords.lat, coords.lng, getGymName(gym));

        const rawPath = getGymImagePath(gym);

        if (carouselItems) {
            if (rawPath && rawPath.trim() !== "") {
                const cleanPath = rawPath.replace("images/", "").replace("images\\", "");
                const dotIndex = cleanPath.lastIndexOf(".");
                const baseName = cleanPath.substring(0, dotIndex);
                const extension = cleanPath.substring(dotIndex);

                const images = [
                    `${SERVER_URL}/images/${cleanPath}`,
                    `${SERVER_URL}/images/${baseName}2${extension}`,
                    `${SERVER_URL}/images/${baseName}3${extension}`
                ];

                carouselItems.innerHTML = "";

                images.forEach((src, index) => {
                    carouselItems.innerHTML += `
                        <div class="carousel-item ${index === 0 ? "active" : ""}">
                            <img src="${src}" class="d-block w-100"
                                 onerror="this.onerror=null; this.src='https://via.placeholder.com/800x400?text=Nincs+k%C3%A9p';"
                                 alt="Konditerem kép ${index + 1}">
                        </div>
                    `;
                });
            } else {
                carouselItems.innerHTML = `
                    <div class="carousel-item active">
                        <img src="https://via.placeholder.com/800x400?text=Nincs+k%C3%A9p" class="d-block w-100" alt="Nincs kép">
                    </div>
                `;
            }
        }

                async function fetchReviews() {
            if (!reviewsContainer) return;

            try {
                const response = await fetch(`${API_URL}/Rating`);
                if (!response.ok) throw new Error("Hiba a vélemények lekérésekor");

                const allReviews = await response.json();
                const gymReviews = allReviews.filter(r => {
                    const ratingGym = r.gym || r.Gym;
                    const ratingGymId = ratingGym?.id || ratingGym?.Id || r.gymId || r.GymId;
                    return parseInt(ratingGymId) === parseInt(id);
                });

                reviewsContainer.innerHTML = "";

                if (gymReviews.length === 0) {
                    reviewsContainer.innerHTML = `<p class="text-muted">Még nincs értékelés.</p>`;
                    return;
                }

                gymReviews.forEach(review => {
                    const stars = review.stars || review.Stars || 0;
                    const message = review.message || review.Message || "";
                    const user = review.user || review.User;
                    const userName = user?.name || user?.Name || "Ismeretlen felhasználó";

                    reviewsContainer.innerHTML += `
                        <div class="mb-3 border-bottom border-secondary pb-3">
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <strong class="text-info">${userName}</strong>
                                <span>${"⭐".repeat(stars)}</span>
                            </div>
                            <p class="mb-0">${message}</p>
                        </div>
                    `;
                });
            } catch (error) {
                console.error("Vélemény betöltési hiba:", error);
                reviewsContainer.innerHTML = `<p class="text-danger">Hiba a vélemények betöltésekor.</p>`;
            }
        }

        await fetchReviews();

        if (reviewForm) {
            reviewForm.addEventListener("submit", async function (e) {
                e.preventDefault();

                const token = localStorage.getItem("token");

                if (!token) {
                    alert("Vélemény írásához be kell jelentkezned!");
                    window.location.href = "login.html";
                    return;
                }

                const starsValue = document.getElementById("review-stars").value;
                const messageValue = document.getElementById("review-text").value.trim();

                const reviewData = {
                    stars: parseInt(starsValue),
                    message: messageValue,
                    userId: 0,
                    gymId: parseInt(id)
                };

                try {
                    const response = await fetch(`${API_URL}/Rating`, {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                            "Authorization": `Bearer ${token}`
                        },
                        body: JSON.stringify(reviewData)
                    });

                    if (response.ok) {
                        alert("Vélemény sikeresen elküldve!");
                        reviewForm.reset();
                        await fetchReviews();
                    } else {
                        let errorMessage = `HTTP ${response.status}`;
                        try {
                            const errorText = await response.text();
                            if (errorText) errorMessage += `\n${errorText}`;
                        } catch {}

                        alert("Hiba történt a küldés során:\n" + errorMessage);
                    }
                } catch (error) {
                    console.error("Vélemény küldési hiba:", error);
                    alert("Nem sikerült elérni a szervert.");
                }
            });
        }

        const buyBtn = document.getElementById("buy-ticket-btn");
        if (buyBtn) {
            buyBtn.onclick = () => {
                window.location.href = `purchase.html?id=${id}`;
            };
        }

        const favBtn = document.getElementById("fav-btn");
        const favIcon = document.getElementById("fav-icon");

        if (favBtn && favIcon) {
            const token = localStorage.getItem("token");
            const favKey = getFavoritesKey();

            if (token && favKey) {
                let favorites = JSON.parse(localStorage.getItem(favKey)) || [];
                if (favorites.includes(parseInt(id))) {
                    favIcon.classList.replace("bi-heart", "bi-heart-fill");
                    favBtn.classList.replace("btn-outline-danger", "btn-danger");
                    favIcon.classList.add("text-white");
                } else {
                    favIcon.classList.replace("bi-heart-fill", "bi-heart");
                    favBtn.classList.replace("btn-danger", "btn-outline-danger");
                    favIcon.classList.remove("text-white");
                }
            } else {
                favIcon.classList.replace("bi-heart-fill", "bi-heart");
                favBtn.classList.replace("btn-danger", "btn-outline-danger");
                favIcon.classList.remove("text-white");
            }

            favBtn.onclick = function () {
                const currentToken = localStorage.getItem("token");
                const currentFavKey = getFavoritesKey();

                if (!currentToken || !currentFavKey) {
                    alert("A kedvencekhez adáshoz be kell jelentkezned!");
                    window.location.href = "login.html";
                    return;
                }

                let currentFavs = JSON.parse(localStorage.getItem(currentFavKey)) || [];
                const index = currentFavs.indexOf(parseInt(id));

                if (index === -1) {
                    currentFavs.push(parseInt(id));
                    favIcon.classList.replace("bi-heart", "bi-heart-fill");
                    favBtn.classList.replace("btn-outline-danger", "btn-danger");
                    favIcon.classList.add("text-white");
                } else {
                    currentFavs.splice(index, 1);
                    favIcon.classList.replace("bi-heart-fill", "bi-heart");
                    favBtn.classList.replace("btn-danger", "btn-outline-danger");
                    favIcon.classList.remove("text-white");
                }

                localStorage.setItem(currentFavKey, JSON.stringify(currentFavs));
            };
        }
    } catch (error) {
        console.error("Hiba a részletek betöltésekor:", error);
    }
}

// --- 4. VÁSÁRLÁS (PURCHASE.HTML) ---
async function handlePurchase() {
    const submitBtn = document.getElementById("submit-purchase");
    if (!submitBtn) return;

    submitBtn.addEventListener("click", async () => {
        const token = localStorage.getItem("token");
        const userId = localStorage.getItem("userId");

        const urlParams = new URLSearchParams(window.location.search);
        const gymId = parseInt(urlParams.get("id")) || 1;

        const priceSelect = document.getElementById("product-select");
        const price = priceSelect ? parseInt(priceSelect.value) : 0;

        if (!token || !userId) {
            alert("A vásárláshoz be kell jelentkezned!");
            window.location.href = "login.html";
            return;
        }

        let productId = 1;
        let expirationDate = null;

        if (price === 2500) {
            productId = 1;
            expirationDate = new Date().toISOString();
        } else if (price === 12000) {
            productId = 2;
            expirationDate = new Date(
                new Date().setMonth(new Date().getMonth() + 1)
            ).toISOString();
        } else if (price === 30000) {
            productId = 3;
            expirationDate = new Date(
                new Date().setMonth(new Date().getMonth() + 3)
            ).toISOString();
        }

        const vasarlasAdatok = {
            isCash: true,
            isCreditCard: false,
            amount: price,
            productId: productId,
            userId: parseInt(userId),
            gymId: parseInt(gymId),
            expirationDate: expirationDate
        };

        try {
            const response = await fetch(`${API_URL}/Purchase`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify(vasarlasAdatok)
            });

            if (response.ok) {
                alert("Sikeres vásárlás! Jó edzést!");
                window.location.href = "profil.html";
            } else {
                let hibaUzenet = `HTTP ${response.status}`;

                try {
                    const errorText = await response.text();
                    if (errorText) hibaUzenet += `\n${errorText}`;
                } catch {}

                alert("Szerver hiba:\n" + hibaUzenet);
            }
        } catch (err) {
            console.error("Hálózati hiba:", err);
            alert("Nem sikerült elérni a szervert. Ellenőrizd a futását!");
        }
    });
}

// --- 5. BÉRLETEK BETÖLTÉSE (PROFIL.HTML) ---
async function loadMyTickets() {
    const container = document.getElementById("my-tickets-container");
    if (!container) return;

    const userId = localStorage.getItem("userId");
    const token = localStorage.getItem("token");

    if (!userId || !token) {
        container.innerHTML = "<p class='text-center text-warning'>Bejelentkezés szükséges a bérletekhez.</p>";
        return;
    }

    try {
        const response = await fetch(`${API_URL}/Purchase/user/${userId}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (response.ok) {
            const data = await response.json();
            container.innerHTML = "";

            if (data.length === 0) {
                container.innerHTML = "<p class='text-center'>Még nincs bérleted.</p>";
                return;
            }

            data.forEach(t => {
                const amount = t.amount || t.Amount;
                const expirationDate = t.expirationDate || t.ExpirationDate;
                const product = t.product || t.Product;
                const gym = t.gym || t.Gym;

                const designation = product?.designation || product?.Designation || "Bérlet / Jegy";
                const gymName = gym?.name || gym?.Name || "Ismeretlen terem";

                container.innerHTML += `
                    <div class="col-md-6 mb-3">
                        <div class="card bg-dark border-primary p-3">
                            <h4 class="text-info">${designation}</h4>
                            <p class="text-info">Terem: ${gymName}</p>
                            <p>Összeg: ${amount} Ft</p>
                            <p class="text-warning">Lejárat: ${expirationDate ? new Date(expirationDate).toLocaleDateString() : 'Nincs adat'}</p>
                        </div>
                    </div>
                `;
            });
        } else {
            throw new Error("Szerver hiba");
        }
    } catch (err) {
        console.error("Hiba a bérletek betöltésekor:", err);
        container.innerHTML = "<p class='text-center text-danger'>Hiba a bérletek betöltésekor.</p>";
    }
}

// --- 5.5 KEDVENCEK BETÖLTÉSE ---
async function loadMyFavorites() {
    const container = document.getElementById("my-favorites-container");
    if (!container) return;

    const favKey = getFavoritesKey();

    if (!favKey) {
        container.innerHTML = '<div class="col-12 text-center"><p class="text-danger fs-5 mt-4">Jelentkezz be a kedvencek megtekintéséhez!</p></div>';
        return;
    }

    let favorites = JSON.parse(localStorage.getItem(favKey)) || [];

    if (favorites.length === 0) {
        container.innerHTML = '<div class="col-12 text-center"><p class="fs-5 mt-4" style="color: #cbd5e1; opacity: 0.85;">Még nem jelöltél meg kedvenc termeket.</p></div>';
        return;
    }

    try {
        const response = await fetch(`${API_URL}/Gym`);
        if (!response.ok) throw new Error("Hiba a termek lekérésekor");

        const allGymsData = await response.json();

        const favoriteGyms = allGymsData.filter(gym => {
            const id = getGymId(gym);
            return favorites.includes(id);
        });

        container.innerHTML = "";

        favoriteGyms.forEach(gym => {
            const name = getGymName(gym);
            const id = getGymId(gym);
            let rawPath = getGymImagePath(gym);
            const cleanPath = rawPath.replace("images/", "").replace("images\\", "");
            const imageSrc = cleanPath ? `${SERVER_URL}/images/${cleanPath}` : "";

            container.innerHTML += `
                <div class="col-12 col-md-6 col-lg-4 mb-4">
                    <div class="card text-white shadow-lg h-100 border-0" style="border-radius: 20px; overflow: hidden; background: #1e293b;">
                        <img src="${imageSrc}" class="card-img-top w-100" style="height: 220px; object-fit: cover;" onerror="this.onerror=null; this.src=''; this.alt='Nincs kép';">
                        <div class="card-body text-center p-4">
                            <h4 class="fw-bold mb-4">${name}</h4>
                            <div class="d-flex justify-content-center gap-3">
                                <a href="gym.html?id=${id}" class="btn btn-outline-light rounded-pill px-4">Részletek</a>
                                <button class="btn btn-danger rounded-pill px-4 remove-fav-btn" data-id="${id}">
                                    Törlés
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });

        document.querySelectorAll(".remove-fav-btn").forEach(btn => {
            btn.addEventListener("click", (e) => {
                const btnElement = e.target.closest("button");
                const removeId = parseInt(btnElement.getAttribute("data-id"));

                let currentFavs = JSON.parse(localStorage.getItem(favKey)) || [];
                currentFavs = currentFavs.filter(fId => fId !== removeId);
                localStorage.setItem(favKey, JSON.stringify(currentFavs));

                loadMyFavorites();
            });
        });
    } catch (error) {
        container.innerHTML = '<div class="alert alert-danger">Hiba a kedvencek betöltésekor.</div>';
    }
}

function getFavoritesKey() {
    const token = localStorage.getItem("token");
    const email = localStorage.getItem("userEmail");
    const userId = localStorage.getItem("userId");

    if (!token) return null;
    if (email && email !== "undefined") return `gymFavorites_${email}`;
    if (userId && userId !== "undefined") return `gymFavorites_user${userId}`;
    return `gymFavorites_${token.substring(0, 15)}`;
}

// --- 6. LOGIN ---
async function login(email, password) {
    try {
        const response = await fetch(`${API_URL}/Auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email: email, password: password })
        });

        if (response.ok) {
            const data = await response.json();

            const token = data.token || data.Token;
            localStorage.setItem("token", token);

            let finalId = data.id || data.userId || (data.user && data.user.id) || data.Id || data.UserId;

            if (finalId) {
                localStorage.setItem("userId", finalId.toString());
            } else {
                localStorage.setItem("userId", "1");
            }

            localStorage.setItem("userEmail", email);

            alert("Sikeres bejelentkezés!");
            window.location.href = "index.html";
        } else {
            alert("Hibás email vagy jelszó!");
        }
    } catch (error) {
        console.error("Login hiba:", error);
        alert("A szerver nem elérhető! Fut a Rider?");
    }
}

function parseJwt(token) {
    try {
        const base64Url = token.split(".")[1];
        const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split("")
                .map(c => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
                .join("")
        );
        return JSON.parse(jsonPayload);
    } catch {
        return null;
    }
}

function getUserRoleFromToken(token) {
    if (!token) return null;
    const payload = parseJwt(token);
    if (!payload) return null;

    return (
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"] ||
        payload.role ||
        payload.Role ||
        null
    );
}

function updateAuthUI() {
    const token = localStorage.getItem("token");
    const role = getUserRoleFromToken(token);

    const loginLink = document.getElementById("login-link");
    const profileLink = document.getElementById("profile-link");
    const adminLink = document.getElementById("admin-link");
    const logoutBtn = document.getElementById("logout-btn");

    if (token) {
        if (loginLink) loginLink.style.display = "none";
        if (profileLink) profileLink.style.display = "inline-block";
        if (logoutBtn) logoutBtn.style.display = "inline-block";

        if (adminLink) {
            adminLink.style.display = role === "admin" ? "inline-block" : "none";
        }
    } else {
        if (loginLink) loginLink.style.display = "inline-block";
        if (profileLink) profileLink.style.display = "none";
        if (logoutBtn) logoutBtn.style.display = "none";
        if (adminLink) adminLink.style.display = "none";
    }
}

document.addEventListener("click", (e) => {
    if (e.target && (e.target.id === "logout-btn" || e.target.id === "logout-btn-profile")) {
        localStorage.removeItem("token");
        localStorage.removeItem("userId");
        localStorage.removeItem("userEmail");
        window.location.href = "index.html";
    }
});

// --- 7. TÉMA VÁLTÓ ---
function toggleTheme() {
    const body = document.body;
    body.classList.toggle("light-mode");
    const isLight = body.classList.contains("light-mode");
    localStorage.setItem("gymFinderTheme", isLight ? "light" : "dark");
}

const savedTheme = localStorage.getItem("gymFinderTheme");
if (savedTheme === "light") {
    document.body.classList.add("light-mode");
}
async function initContactMap() {
    const address = "Szombathely, Zrínyi Ilona utca 12";

    const map = L.map("contact-map").setView([47.23, 16.62], 13);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap"
    }).addTo(map);

    try {
        const response = await fetch(
            `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}`
        );

        const data = await response.json();

        if (data.length === 0) {
            console.error("Nem található cím");
            return;
        }

        const lat = parseFloat(data[0].lat);
        const lng = parseFloat(data[0].lon);

        map.setView([lat, lng], 17);

        const marker = L.marker([lat, lng]).addTo(map);

        marker.bindPopup(`
            <b>Gym Finder</b><br>
            ${address}
        `).openPopup();

        document.getElementById("address").addEventListener("click", () => {
            map.flyTo([lat, lng], 18);
            marker.openPopup();
        });

    } catch (err) {
        console.error("Geocoding hiba:", err);
    }

    setTimeout(() => {
        map.invalidateSize();
    }, 300);
}