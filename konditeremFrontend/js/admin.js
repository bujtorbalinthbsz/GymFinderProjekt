const SERVER_URL = "http://localhost:5131";
const API_URL = `${SERVER_URL}/api/Gym`;

let adminGyms = [];
let filteredGyms = [];
let editModalInstance = null;

document.addEventListener("DOMContentLoaded", () => {
    const adminPanel = document.getElementById("admin-panel");
    const deniedBox = document.getElementById("admin-access-denied");
    const roleStatus = document.getElementById("admin-role-status");
    const refreshBtn = document.getElementById("refresh-admin-btn");
    const saveBtn = document.getElementById("save-edit-btn");
    const logoutBtn = document.getElementById("admin-logout-btn");
    const searchInput = document.getElementById("admin-search");
    const imageInput = document.getElementById("edit-imagePath");

    const token = localStorage.getItem("token");
    const role = getUserRoleFromToken(token);

    if (!token || role !== "admin") {
        if (adminPanel) adminPanel.classList.add("d-none-force");
        if (deniedBox) deniedBox.classList.remove("d-none-force");
        if (roleStatus) {
            roleStatus.innerHTML = `<span class="text-danger fw-bold">Nincs admin jogosultság</span>`;
        }
        return;
    }

    if (roleStatus) {
        roleStatus.innerHTML = `<span class="text-success fw-bold">Admin jogosultság aktív</span>`;
    }

    const modalElement = document.getElementById("editGymModal");
    if (modalElement) {
        editModalInstance = new bootstrap.Modal(modalElement);
    }

    loadAdminGyms();

    if (refreshBtn) {
        refreshBtn.addEventListener("click", loadAdminGyms);
    }

    if (saveBtn) {
        saveBtn.addEventListener("click", saveEdit);
    }

    if (logoutBtn) {
        logoutBtn.addEventListener("click", logoutAdmin);
    }

    if (searchInput) {
        searchInput.addEventListener("input", e => {
            applyFilter(e.target.value);
        });
    }

    if (imageInput) {
        imageInput.addEventListener("input", e => {
            updateImagePreview(e.target.value);
        });
    }
});

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

function getAuthHeaders() {
    const token = localStorage.getItem("token");

    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
    };
}

function logoutAdmin() {
    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    localStorage.removeItem("userEmail");
    window.location.href = "index.html";
}

function getGymField(gym, ...keys) {
    for (const key of keys) {
        if (gym[key] !== undefined && gym[key] !== null) {
            return gym[key];
        }
    }
    return "";
}

function formatOpenAt(openAt) {
    if (!openAt) return "";

    if (typeof openAt === "string") return openAt;

    return (
        openAt.info ||
        openAt.Info ||
        openAt.name ||
        openAt.Name ||
        openAt.value ||
        openAt.Value ||
        JSON.stringify(openAt)
    );
}

function updateStats() {
    const totalCount = adminGyms.length;
    const imageCount = adminGyms.filter(g => {
        const imagePath = getGymField(g, "imagePath", "ImagePath");
        return !!imagePath && imagePath.trim() !== "";
    }).length;

    const filteredCount = filteredGyms.length;

    const countEl = document.getElementById("gym-count");
    const imageCountEl = document.getElementById("gym-image-count");
    const filteredEl = document.getElementById("gym-filter-count");

    if (countEl) countEl.textContent = totalCount;
    if (imageCountEl) imageCountEl.textContent = imageCount;
    if (filteredEl) filteredEl.textContent = filteredCount;
}

async function loadAdminGyms() {
    const tbody = document.getElementById("admin-tbody");

    if (tbody) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4">Adatok betöltése...</td></tr>`;
    }

    try {
        const response = await fetch(API_URL, {
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || `HTTP ${response.status}`);
        }

        adminGyms = await response.json();
        filteredGyms = [...adminGyms];

        renderAdminTable();
        updateStats();
    } catch (error) {
        console.error("Hiba a termek betöltésekor:", error);

        if (tbody) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center text-danger py-4">
                        Hiba a termek betöltésekor.
                    </td>
                </tr>
            `;
        }
    }
}

function applyFilter(searchText = "") {
    const term = searchText.trim().toLowerCase();

    filteredGyms = adminGyms.filter(gym => {
        const name = String(getGymField(gym, "name", "Name")).toLowerCase();
        const phone = String(getGymField(gym, "phone", "Phone")).toLowerCase();
        const email = String(getGymField(gym, "email", "Email")).toLowerCase();

        return name.includes(term) || phone.includes(term) || email.includes(term);
    });

    renderAdminTable();
    updateStats();
}

function renderAdminTable() {
    const tbody = document.getElementById("admin-tbody");
    if (!tbody) return;

    tbody.innerHTML = "";

    if (!filteredGyms.length) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7">
                    <div class="empty-box">
                        <div class="fs-5 fw-bold mb-2">Nincs találat</div>
                        <div>Próbálj meg másik keresőkifejezést.</div>
                    </div>
                </td>
            </tr>
        `;
        return;
    }

    filteredGyms.forEach(gym => {
        const id = getGymField(gym, "id", "Id");
        const name = getGymField(gym, "name", "Name");
        const openAt = formatOpenAt(getGymField(gym, "openAt", "OpenAt"));
        const phone = getGymField(gym, "phone", "Phone");
        const email = getGymField(gym, "email", "Email");
        const imagePath = getGymField(gym, "imagePath", "ImagePath");

        tbody.innerHTML += `
            <tr>
                <td class="muted-cell">#${id}</td>
                <td class="gym-name-cell">${escapeHtml(name)}</td>
                <td>${escapeHtml(openAt || "Nincs adat")}</td>
                <td>${escapeHtml(phone || "Nincs adat")}</td>
                <td>${escapeHtml(email || "Nincs adat")}</td>
                <td>
                    ${imagePath
                        ? `<span class="image-badge"><i class="bi bi-image"></i>${escapeHtml(imagePath)}</span>`
                        : `<span class="muted-cell">Nincs kép</span>`
                    }
                </td>
                <td>
                    <div class="action-group">
                        <button class="btn btn-sm btn-soft-info" onclick="openEdit(${id})">
                            <i class="bi bi-pencil-square"></i>
                        </button>
                        <button class="btn btn-sm btn-soft-danger" onclick="deleteGym(${id})">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    });
}

function openEdit(id) {
    const gym = adminGyms.find(g => parseInt(getGymField(g, "id", "Id")) === parseInt(id));
    if (!gym) return;

    document.getElementById("edit-id").value = getGymField(gym, "id", "Id");
    document.getElementById("edit-name").value = getGymField(gym, "name", "Name");
    document.getElementById("edit-openAt").value = formatOpenAt(getGymField(gym, "openAt", "OpenAt"));
    document.getElementById("edit-phone").value = getGymField(gym, "phone", "Phone");
    document.getElementById("edit-email").value = getGymField(gym, "email", "Email");
    document.getElementById("edit-imagePath").value = getGymField(gym, "imagePath", "ImagePath");

    updateImagePreview(getGymField(gym, "imagePath", "ImagePath"));

    if (editModalInstance) {
        editModalInstance.show();
    }
}

function updateImagePreview(imagePath) {
    const previewBox = document.getElementById("image-preview-box");
    if (!previewBox) return;

    const cleanPath = (imagePath || "").trim().replace("images/", "").replace("images\\", "");

    if (!cleanPath) {
        previewBox.innerHTML = `<div class="preview-placeholder">Nincs kiválasztva kép</div>`;
        return;
    }

    const fullSrc = `${SERVER_URL}/images/${cleanPath}`;

    previewBox.innerHTML = `
        <img src="${fullSrc}" alt="Előnézet"
             onerror="this.style.display='none'; this.parentElement.innerHTML='<div class=&quot;preview-placeholder&quot;>A kép nem található.</div>';">
    `;
}

async function saveEdit() {
    const id = parseInt(document.getElementById("edit-id").value);

    const updatedGym = {
        id: id,
        name: document.getElementById("edit-name").value.trim(),
        openAt: {
            info: document.getElementById("edit-openAt").value.trim()
        },
        phone: document.getElementById("edit-phone").value.trim(),
        email: document.getElementById("edit-email").value.trim(),
        imagePath: document.getElementById("edit-imagePath").value.trim()
    };

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: "PUT",
            headers: getAuthHeaders(),
            body: JSON.stringify(updatedGym)
        });

        if (response.ok) {
            alert("Terem sikeresen frissítve!");
            if (editModalInstance) editModalInstance.hide();
            loadAdminGyms();
        } else {
            const err = await response.text();
            alert("Hiba a mentésnél:\n" + err);
        }
    } catch (error) {
        console.error("Szerver hiba mentéskor:", error);
        alert("Nem sikerült elérni a szervert.");
    }
}

async function deleteGym(id) {
    if (!confirm("Biztosan törölni akarod ezt a termet?")) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: "DELETE",
            headers: getAuthHeaders()
        });

        if (response.ok) {
            alert("Terem sikeresen törölve!");
            loadAdminGyms();
        } else {
            const err = await response.text();
            alert("Hiba történt a törlés során:\n" + err);
        }
    } catch (error) {
        console.error("Szerver hiba törléskor:", error);
        alert("Nem sikerült elérni a szervert.");
    }
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}