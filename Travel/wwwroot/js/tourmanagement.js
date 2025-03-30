// tourmanagement.js
class TourManager {
    constructor() {
        this.tours = [
            {
                id: 1,
                name: "Đà Lạt - Thành phố ngàn hoa",
                location: "Đà Lạt",
                startDate: "2023-06-15",
                endDate: "2023-06-18",
                price: 2500000,
                totalSlots: 30,
                availableSlots: 20,
                status: "Còn chỗ",
                description: "Khám phá thành phố ngàn hoa với đồi thông bạt ngàn, những vườn hoa rực rỡ và không khí trong lành."
            },
            {
                id: 2,
                name: "Nha Trang - Biển xanh cát trắng",
                location: "Nha Trang",
                startDate: "2023-06-20",
                endDate: "2023-06-23",
                price: 3200000,
                totalSlots: 25,
                availableSlots: 15,
                status: "Còn chỗ",
                description: "Thưởng thức bãi biển xanh, cát trắng và nắng vàng tại Nha Trang."
            }
        ];

        this.selectedTour = null;
        this.initModals();
        this.initEventListeners();
        this.updateTourTable();
    }

    async addTour() {
        const addBtn = document.querySelector('#addTourModal .btn-primary');
        const addModal = document.getElementById('addTourModal');

        try {
            // Hiển thị trạng thái loading
            addModal.classList.add('modal-loading');
            addBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang thêm...';
            addBtn.disabled = true;

            // Lấy giá trị từ form
            const tourName = document.getElementById('tourName').value;
            const tourLocation = document.getElementById('tourLocation').value;
            const startDate = document.getElementById('startDate').value;
            const endDate = document.getElementById('endDate').value;
            const tourPrice = document.getElementById('tourPrice').value;
            const totalSlots = document.getElementById('totalSlots').value;
            const tourStatus = document.getElementById('tourStatus').value;
            const tourDescription = document.getElementById('tourDescription').value;

            // Validate dữ liệu
            if (!tourName || !tourLocation || !startDate || !endDate || !tourPrice || !totalSlots) {
                await Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Vui lòng điền đầy đủ thông tin bắt buộc!',
                });
                return;
            }

            // Giả lập API call
            await new Promise(resolve => setTimeout(resolve, 500));

            // Tạo tour mới
            const newTour = {
                id: this.tours.length > 0 ? Math.max(...this.tours.map(t => t.id)) + 1 : 1,
                name: tourName,
                location: tourLocation,
                startDate: startDate,
                endDate: endDate,
                price: parseInt(tourPrice),
                totalSlots: parseInt(totalSlots),
                availableSlots: tourStatus === "1" ? parseInt(totalSlots) : 0,
                status: tourStatus === "1" ? "Còn chỗ" : "Hết chỗ",
                description: tourDescription
            };

            // Thêm vào mảng tours
            this.tours.push(newTour);
            this.updateTourTable();
            this.addTourModal.hide();

            // Hiển thị thông báo thành công
            await Swal.fire({
                icon: 'success',
                title: 'Thành công',
                text: 'Thêm tour mới thành công!',
                timer: 1500,
                showConfirmButton: false
            });

            // Reset form
            document.getElementById('addTourModal').querySelector('form').reset();
        } catch (error) {
            console.error('Lỗi khi thêm tour:', error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: 'Đã xảy ra lỗi khi thêm tour!',
            });
        } finally {
            // Khôi phục trạng thái nút
            addBtn.innerHTML = 'Lưu Tour';
            addBtn.disabled = false;
            addModal.classList.remove('modal-loading');
            // Loại bỏ lớp modal-backdrop
            document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
        }
    }

    async updateTour() {
        const editBtn = document.querySelector('#editTourModal .btn-warning');
        const editModal = document.getElementById('editTourModal');

        try {
            // Hiển thị trạng thái loading
            editModal.classList.add('modal-loading');
            editBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang cập nhật...';
            editBtn.disabled = true;

            // Lấy giá trị từ form
            const tourName = document.getElementById('editTourName').value;
            const tourLocation = document.getElementById('editTourLocation').value;
            const startDate = document.getElementById('editStartDate').value;
            const endDate = document.getElementById('editEndDate').value;
            const tourPrice = document.getElementById('editTourPrice').value;
            const totalSlots = document.getElementById('editTotalSlots').value;
            const tourStatus = document.getElementById('editTourStatus').value;
            const tourDescription = document.getElementById('editTourDescription').value;

            // Validate dữ liệu
            if (!tourName || !tourLocation || !startDate || !endDate || !tourPrice || !totalSlots) {
                await Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Vui lòng điền đầy đủ thông tin bắt buộc!',
                });
                return;
            }

            // Giả lập API call
            await new Promise(resolve => setTimeout(resolve, 500));

            // Cập nhật tour
            this.selectedTour.name = tourName;
            this.selectedTour.location = tourLocation;
            this.selectedTour.startDate = startDate;
            this.selectedTour.endDate = endDate;
            this.selectedTour.price = parseInt(tourPrice);
            this.selectedTour.totalSlots = parseInt(totalSlots);
            this.selectedTour.availableSlots = tourStatus === "1" ? parseInt(totalSlots) : 0;
            this.selectedTour.status = tourStatus === "1" ? "Còn chỗ" : "Hết chỗ";
            this.selectedTour.description = tourDescription;

            // Cập nhật mảng tours
            const index = this.tours.findIndex(t => t.id === this.selectedTour.id);
            if (index !== -1) {
                this.tours[index] = this.selectedTour;
            }
            this.updateTourTable();
            this.editTourModal.hide();

            // Hiển thị thông báo thành công
            await Swal.fire({
                icon: 'success',
                title: 'Thành công',
                text: 'Cập nhật tour thành công!',
                timer: 1500,
                showConfirmButton: false
            });

            // Reset form
            document.getElementById('editTourModal').querySelector('form').reset();
        } catch (error) {
            console.error('Lỗi khi cập nhật tour:', error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: 'Đã xảy ra lỗi khi cập nhật tour!',
            });
        } finally {
            // Khôi phục trạng thái nút
            editBtn.innerHTML = 'Lưu thay đổi';
            editBtn.disabled = false;
            editModal.classList.remove('modal-loading');
            // Loại bỏ lớp modal-backdrop
            document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
        }
    }

    initModals() {
        this.addTourModal = new bootstrap.Modal(document.getElementById('addTourModal'));
        this.editTourModal = new bootstrap.Modal(document.getElementById('editTourModal'));
        this.deleteTourModal = new bootstrap.Modal(document.getElementById('deleteTourModal'));
    }

    initEventListeners() {
        // Nút thêm tour
        document.querySelector('#addTourModal .btn-primary').addEventListener('click', () => this.addTour());

        // Nút cập nhật tour
        document.querySelector('#editTourModal .btn-warning').addEventListener('click', () => this.updateTour());

        // Nút xóa tour
        document.querySelector('#deleteTourModal .btn-danger').addEventListener('click', () => this.deleteTour());

        // Xử lý khi modal xóa đóng
        document.getElementById('deleteTourModal').addEventListener('hidden.bs.modal', () => {
            const deleteBtn = document.querySelector('#deleteTourModal .btn-danger');
            deleteBtn.innerHTML = 'Xóa Tour';
            deleteBtn.disabled = false;
        });
    }

    async deleteTour() {
        const deleteBtn = document.querySelector('#deleteTourModal .btn-danger');
        const deleteModal = document.getElementById('deleteTourModal');

        try {
            // Hiển thị trạng thái loading
            deleteModal.classList.add('modal-loading');
            deleteBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xóa...';
            deleteBtn.disabled = true;

            // Giả lập API call
            await new Promise(resolve => setTimeout(resolve, 800));

            // Xóa tour
            this.tours = this.tours.filter(t => t.id !== this.selectedTour.id);
            this.updateTourTable();
            this.deleteTourModal.hide();

            // Hiển thị thông báo
            await Swal.fire({
                icon: 'success',
                title: 'Thành công',
                text: 'Đã xóa tour thành công!',
                timer: 1500,
                showConfirmButton: false
            });
        } catch (error) {
            console.error('Lỗi khi xóa tour:', error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: 'Đã xảy ra lỗi khi xóa tour!',
            });
        } finally {
            deleteModal.classList.remove('modal-loading');
            // Loại bỏ lớp modal-backdrop
            document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
        }
    }

    updateTourTable() {
        const tbody = document.querySelector('#tourTable tbody');
        tbody.innerHTML = '';

        if (this.tours.length === 0) {
            const row = document.createElement('tr');
            row.innerHTML = '<td colspan="8" class="text-center text-muted py-4">Không có tour nào</td>';
            tbody.appendChild(row);
            return;
        }

        this.tours.forEach(tour => {
            const row = document.createElement('tr');
            row.innerHTML = `
        <td>${tour.id}</td>
        <td>${tour.name}</td>
        <td>${tour.location}</td>
        <td>${this.formatDate(tour.startDate)}</td>
        <td>${tour.price.toLocaleString()}</td>
        <td>${tour.availableSlots}/${tour.totalSlots}</td>
        <td><span class="badge ${tour.status === 'Còn chỗ' ? 'bg-success' : 'bg-danger'}">${tour.status}</span></td>
        <td>
          <button class="btn btn-sm btn-warning me-1 edit-btn">
            <i class="fas fa-edit"></i>
          </button>
          <button class="btn btn-sm btn-danger delete-btn">
            <i class="fas fa-trash"></i>
          </button>
        </td>
      `;
            tbody.appendChild(row);
        });

        this.attachRowEventHandlers();
    }

    attachRowEventHandlers() {
        // Sự kiện cho nút Sửa
        document.querySelectorAll('.edit-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const row = e.target.closest('tr');
                this.selectedTour = {
                    id: parseInt(row.cells[0].textContent),
                    name: row.cells[1].textContent,
                    location: row.cells[2].textContent,
                    startDate: this.formatDateToInput(row.cells[3].textContent),
                    price: parseInt(row.cells[4].textContent.replace(/,/g, '')),
                    totalSlots: parseInt(row.cells[5].textContent.split('/')[1]),
                    status: row.cells[6].querySelector('span').textContent === "Còn chỗ" ? "1" : "0"
                };

                // Điền dữ liệu vào form sửa
                document.getElementById('editTourName').value = this.selectedTour.name;
                document.getElementById('editTourLocation').value = this.selectedTour.location;
                document.getElementById('editStartDate').value = this.selectedTour.startDate;
                document.getElementById('editEndDate').value = this.selectedTour.endDate;
                document.getElementById('editTourPrice').value = this.selectedTour.price;
                document.getElementById('editTotalSlots').value = this.selectedTour.totalSlots;
                document.getElementById('editTourStatus').value = this.selectedTour.status;
                document.getElementById('editTourDescription').value = this.selectedTour.description;

                // Mở modal sửa
                this.editTourModal.show();
            });
        });

        // Sự kiện cho nút Xóa
        document.querySelectorAll('.delete-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const row = e.target.closest('tr');
                this.selectedTour = {
                    id: parseInt(row.cells[0].textContent),
                    name: row.cells[1].textContent
                };

                document.querySelector('#deleteTourModal strong').textContent = `"${this.selectedTour.name}"`;
                this.deleteTourModal.show();
            });
        });
    }

    formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    formatDateToInput(dateString) {
        if (!dateString) return '';
        const [day, month, year] = dateString.split('/');
        return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
    }
}

// Khởi tạo khi DOM ready
document.addEventListener('DOMContentLoaded', () => {
    new TourManager();
});
