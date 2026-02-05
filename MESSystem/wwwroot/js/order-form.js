// 주문서 작성 폼 JavaScript

let rowIndex = 5; // 초기 5줄

// 페이지 로드 시 초기화
document.addEventListener('DOMContentLoaded', function() {
    initializeShippingMethodToggle();
    initializeProductSelect();
    initializeRowButtons();
    updateRowCount();
});

// 출고방법 선택 시 필드 토글
function initializeShippingMethodToggle() {
    const shippingMethodSelect = document.getElementById('shippingMethod');
    const paymentMethodGroup = document.getElementById('paymentMethodGroup');
    const shippingTimeGroup = document.getElementById('shippingTimeGroup');
    const paymentRequired = document.getElementById('paymentRequired');
    const timeRequired = document.getElementById('timeRequired');

    shippingMethodSelect.addEventListener('change', function() {
        const method = this.value;
        
        // 결제방법 필드 표시/숨김
        if (['대신택배', '대신화물', '한진택배', '퀵', '용차'].includes(method)) {
            paymentMethodGroup.style.display = 'block';
            paymentRequired.style.display = 'inline';
        } else {
            paymentMethodGroup.style.display = 'none';
            paymentRequired.style.display = 'none';
            document.getElementById('paymentMethod').value = '';
        }

        // 출고시간 필드 표시/숨김
        if (['퀵', '용차', '방문수령', '직접배송'].includes(method)) {
            shippingTimeGroup.style.display = 'block';
            timeRequired.style.display = 'inline';
        } else {
            shippingTimeGroup.style.display = 'none';
            timeRequired.style.display = 'none';
            document.querySelector('input[name="Input.ShippingTime"]').value = '';
        }
    });
}

// 품목 선택 시 기본 규격 자동 입력
function initializeProductSelect() {
    document.addEventListener('change', function(e) {
        if (e.target.classList.contains('product-select')) {
            const selectedOption = e.target.options[e.target.selectedIndex];
            const defaultSpec = selectedOption.getAttribute('data-spec');
            
            // 같은 행의 규격 입력란 찾기
            const row = e.target.closest('tr');
            const specInput = row.querySelector('.spec-input');
            
            if (defaultSpec && specInput) {
                specInput.value = defaultSpec;
            }
        }
    });
}

// 줄 추가 버튼
function initializeRowButtons() {
    const addRowBtn = document.getElementById('addRowBtn');
    const itemsTableBody = document.getElementById('itemsTableBody');

    addRowBtn.addEventListener('click', function() {
        const currentRows = document.querySelectorAll('.item-row').length;
        
        if (currentRows >= 20) {
            alert('최대 20줄까지만 추가할 수 있습니다.');
            return;
        }

        addRow();
    });

    // 줄 삭제 버튼 (이벤트 위임)
    itemsTableBody.addEventListener('click', function(e) {
        if (e.target.closest('.remove-row-btn')) {
            const row = e.target.closest('.item-row');
            removeRow(row);
        }
    });
}

// 줄 추가 함수
function addRow() {
    const tbody = document.getElementById('itemsTableBody');
    const newRow = document.createElement('tr');
    newRow.className = 'item-row';
    newRow.setAttribute('data-row-index', rowIndex);

    // 품목 옵션 HTML 생성
    const productOptions = Array.from(document.querySelector('.product-select').options)
        .map(opt => `<option value="${opt.value}" data-spec="${opt.getAttribute('data-spec') || ''}">${opt.text}</option>`)
        .join('');

    newRow.innerHTML = `
        <td class="text-center row-number">${rowIndex + 1}</td>
        <td>
            <select name="Input.Items[${rowIndex}].ProductId" class="form-select form-select-sm product-select">
                ${productOptions}
            </select>
        </td>
        <td>
            <input type="text" name="Input.Items[${rowIndex}].Spec" class="form-control form-control-sm spec-input" placeholder="예: 1000x2000mm" />
        </td>
        <td>
            <input type="text" name="Input.Items[${rowIndex}].Description" class="form-control form-control-sm" placeholder="선택 입력" />
        </td>
        <td>
            <input type="number" name="Input.Items[${rowIndex}].Quantity" class="form-control form-control-sm" min="1" placeholder="0" />
        </td>
        <td>
            <input type="number" name="Input.Items[${rowIndex}].UnitPrice" class="form-control form-control-sm" min="0" placeholder="0" />
        </td>
        <td>
            <input type="file" name="Input.Items[${rowIndex}].DesignFile" class="form-control form-control-sm" accept=".ai,.psd,.pdf,.eps,.jpg,.png" />
        </td>
        <td>
            <input type="text" name="Input.Items[${rowIndex}].Remark" class="form-control form-control-sm" placeholder="선택 입력" />
        </td>
        <td class="text-center">
            <button type="button" class="btn btn-sm btn-outline-danger remove-row-btn">
                <i class="bi bi-x"></i>
            </button>
        </td>
    `;

    tbody.appendChild(newRow);
    rowIndex++;
    
    updateRowCount();
    updateRemoveButtons();
}

// 줄 삭제 함수
function removeRow(row) {
    row.remove();
    reindexRows();
    updateRowCount();
    updateRemoveButtons();
}

// 행 번호 재정렬
function reindexRows() {
    const rows = document.querySelectorAll('.item-row');
    rows.forEach((row, index) => {
        row.setAttribute('data-row-index', index);
        row.querySelector('.row-number').textContent = index + 1;
        
        // name 속성 재정렬
        row.querySelectorAll('input, select').forEach(input => {
            const name = input.getAttribute('name');
            if (name) {
                const newName = name.replace(/\[\d+\]/, `[${index}]`);
                input.setAttribute('name', newName);
            }
        });
    });
    
    rowIndex = rows.length;
}

// 현재 줄 수 업데이트
function updateRowCount() {
    const count = document.querySelectorAll('.item-row').length;
    document.getElementById('currentRowCount').textContent = count;
    
    // 최대 줄 도달 시 추가 버튼 비활성화
    const addRowBtn = document.getElementById('addRowBtn');
    if (count >= 20) {
        addRowBtn.disabled = true;
        addRowBtn.innerHTML = '<i class="bi bi-x-circle"></i> 최대 줄 수 도달';
    } else {
        addRowBtn.disabled = false;
        addRowBtn.innerHTML = '<i class="bi bi-plus-circle"></i> 줄 추가';
    }
}

// 삭제 버튼 표시/숨김 (5줄 이하면 숨김)
function updateRemoveButtons() {
    const rows = document.querySelectorAll('.item-row');
    const removeButtons = document.querySelectorAll('.remove-row-btn');
    
    if (rows.length <= 5) {
        removeButtons.forEach(btn => btn.style.display = 'none');
    } else {
        removeButtons.forEach(btn => btn.style.display = 'inline-block');
    }
}

// 폼 제출 전 검증
document.getElementById('orderForm').addEventListener('submit', function(e) {
    const shippingMethod = document.getElementById('shippingMethod').value;
    const paymentMethod = document.getElementById('paymentMethod').value;
    const shippingTime = document.querySelector('input[name="Input.ShippingTime"]').value;

    // 결제방법 필수 검증
    if (['대신택배', '대신화물', '한진택배', '퀵', '용차'].includes(shippingMethod) && !paymentMethod) {
        alert('결제방법을 선택해주세요.');
        e.preventDefault();
        return false;
    }

    // 출고시간 필수 검증
    if (['퀵', '용차', '방문수령', '직접배송'].includes(shippingMethod) && !shippingTime) {
        alert('출고시간을 입력해주세요.');
        e.preventDefault();
        return false;
    }

    // 최소 1개 품목 검증
    let hasItem = false;
    document.querySelectorAll('.product-select').forEach(select => {
        if (select.value) hasItem = true;
    });

    if (!hasItem) {
        alert('최소 1개 이상의 품목을 선택해주세요.');
        e.preventDefault();
        return false;
    }

    // 제출 버튼 비활성화 (중복 제출 방지)
    const submitBtn = document.getElementById('submitBtn');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>저장 중...';
});
