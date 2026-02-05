// 주문서 작성 폼 JavaScript

let rowIndex = 5; // 초기 5줄
let clientSearchTimeout = null;

// 페이지 로드 시 초기화
document.addEventListener('DOMContentLoaded', function() {
    initializeClientAutocomplete();
    initializeProductSearch();
    initializeShippingMethodToggle();
    initializeProductSelect();
    initializeRowButtons();
    updateRowCount();
});

// 거래처 자동완성 초기화
function initializeClientAutocomplete() {
    const clientNameInput = document.getElementById('clientName');
    const clientSuggestions = document.getElementById('clientSuggestions');
    
    if (!clientNameInput) return;

    // 입력 시 검색
    clientNameInput.addEventListener('input', function() {
        const query = this.value.trim();
        
        // 이전 타이머 취소
        if (clientSearchTimeout) {
            clearTimeout(clientSearchTimeout);
        }
        
        if (query.length < 2) {
            clientSuggestions.style.display = 'none';
            return;
        }
        
        // 300ms 후에 검색 (타이핑 완료 대기)
        clientSearchTimeout = setTimeout(() => {
            searchClients(query);
        }, 300);
    });
    
    // 입력란 외부 클릭 시 자동완성 닫기
    document.addEventListener('click', function(e) {
        if (!clientNameInput.contains(e.target) && !clientSuggestions.contains(e.target)) {
            clientSuggestions.style.display = 'none';
        }
    });
}

// 거래처 검색 API 호출
function searchClients(query) {
    fetch(`/api/clients/search?q=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(clients => {
            displayClientSuggestions(clients);
        })
        .catch(error => {
            console.error('거래처 검색 오류:', error);
        });
}

// 거래처 자동완성 결과 표시
function displayClientSuggestions(clients) {
    const clientSuggestions = document.getElementById('clientSuggestions');
    
    if (clients.length === 0) {
        clientSuggestions.style.display = 'none';
        return;
    }
    
    clientSuggestions.innerHTML = clients.map(client => `
        <button type="button" class="list-group-item list-group-item-action" 
                onclick="selectClient(${client.id}, '${escapeHtml(client.name)}', '${escapeHtml(client.address || '')}', '${escapeHtml(client.phone || '')}', '${escapeHtml(client.mobile || '')}')">
            <div class="d-flex w-100 justify-content-between">
                <h6 class="mb-1">${escapeHtml(client.name)}</h6>
                <small>${escapeHtml(client.phone || client.mobile || '')}</small>
            </div>
            ${client.address ? `<small class="text-muted">${escapeHtml(client.address)}</small>` : ''}
        </button>
    `).join('');
    
    clientSuggestions.style.display = 'block';
}

// 거래처 선택 시 폼에 자동 입력
function selectClient(id, name, address, phone, mobile) {
    document.getElementById('clientId').value = id;
    document.getElementById('clientName').value = name;
    document.querySelector('input[name="Input.ClientName"]').value = name;
    
    // 주소
    const addressInput = document.querySelector('input[name="Input.ClientAddress"]');
    if (addressInput && address) {
        addressInput.value = address;
    }
    
    // 전화번호
    const phoneInput = document.querySelector('input[name="Input.ClientPhone"]');
    if (phoneInput && phone) {
        phoneInput.value = phone;
    }
    
    // 휴대폰
    const mobileInput = document.querySelector('input[name="Input.ClientMobile"]');
    if (mobileInput && mobile) {
        mobileInput.value = mobile;
    }
    
    // 자동완성 닫기
    document.getElementById('clientSuggestions').style.display = 'none';
    
    // 성공 알림
    showToast('거래처 정보가 자동으로 입력되었습니다.', 'success');
}

// HTML 이스케이프 (XSS 방지)
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// 토스트 알림 표시
function showToast(message, type = 'info') {
    // Bootstrap Alert로 간단하게 표시
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(alertDiv);
    
    // 3초 후 자동 제거
    setTimeout(() => {
        alertDiv.remove();
    }, 3000);
}

// ==================== 품목 검색 기능 ====================

let productSearchTimeout = null;

// 품목 검색 초기화
function initializeProductSearch() {
    // 이벤트 위임으로 동적 추가 행에도 적용
    document.addEventListener('input', function(e) {
        if (e.target.classList.contains('product-search-input')) {
            handleProductSearch(e.target);
        }
    });
    
    // 외부 클릭 시 자동완성 닫기
    document.addEventListener('click', function(e) {
        if (!e.target.classList.contains('product-search-input')) {
            document.querySelectorAll('.product-suggestions').forEach(div => {
                div.style.display = 'none';
            });
        }
    });
}

// 품목 검색 처리
function handleProductSearch(input) {
    const query = input.value.trim();
    const row = input.closest('tr');
    const suggestions = row.querySelector('.product-suggestions');
    
    // 이전 타이머 취소
    if (productSearchTimeout) {
        clearTimeout(productSearchTimeout);
    }
    
    if (query.length < 2) {
        suggestions.style.display = 'none';
        return;
    }
    
    // 300ms 후에 검색
    productSearchTimeout = setTimeout(() => {
        searchProducts(query, suggestions, row);
    }, 300);
}

// 품목 검색 API 호출
function searchProducts(query, suggestionsDiv, row) {
    fetch(`/api/products/search?q=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(products => {
            displayProductSuggestions(products, suggestionsDiv, row);
        })
        .catch(error => {
            console.error('품목 검색 오류:', error);
        });
}

// 품목 자동완성 결과 표시
function displayProductSuggestions(products, suggestionsDiv, row) {
    if (products.length === 0) {
        suggestionsDiv.innerHTML = '<div class="p-2 text-muted">검색 결과가 없습니다.</div>';
        suggestionsDiv.style.display = 'block';
        return;
    }
    
    suggestionsDiv.innerHTML = products.map(product => `
        <button type="button" class="list-group-item list-group-item-action p-2" 
                style="cursor: pointer; border: none; border-bottom: 1px solid #ddd;"
                onclick='selectProduct(${JSON.stringify(product)}, this)'>
            <div class="d-flex justify-content-between">
                <div>
                    <strong>${escapeHtml(product.name)}</strong>
                    <br>
                    <small class="text-muted">코드: ${escapeHtml(product.code)} | 분류: ${escapeHtml(product.categoryName)}</small>
                </div>
                ${product.defaultSpec ? `<small class="text-info">${escapeHtml(product.defaultSpec)}</small>` : ''}
            </div>
        </button>
    `).join('');
    
    suggestionsDiv.style.display = 'block';
}

// 품목 선택 시 폼에 자동 입력
function selectProduct(product, button) {
    const row = button.closest('tr');
    
    // 품목 ID 저장
    const productIdInput = row.querySelector('.product-id-input');
    productIdInput.value = product.id;
    
    // 검색 입력란에 선택된 품목명 표시
    const searchInput = row.querySelector('.product-search-input');
    searchInput.value = `[${product.code}] ${product.name}`;
    searchInput.setAttribute('data-product-id', product.id);
    
    // 기본 규격 자동 입력
    if (product.defaultSpec) {
        const specInput = row.querySelector('.spec-input');
        if (specInput) {
            specInput.value = product.defaultSpec;
        }
    }
    
    // 자동완성 닫기
    const suggestions = row.querySelector('.product-suggestions');
    suggestions.style.display = 'none';
    
    // 성공 알림
    showToast(`${product.name} 품목이 선택되었습니다.`, 'success');
}

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

    newRow.innerHTML = `
        <td class="text-center row-number">${rowIndex + 1}</td>
        <td class="position-relative">
            <input type="text" 
                   class="form-control form-control-sm product-search-input" 
                   placeholder="품목명 또는 코드 검색..."
                   autocomplete="off"
                   data-row="${rowIndex}" />
            <input type="hidden" name="Input.Items[${rowIndex}].ProductId" class="product-id-input" />
            <div class="product-suggestions position-absolute bg-white border rounded shadow-sm" 
                 style="display:none; z-index: 1000; max-height: 200px; overflow-y: auto; width: 100%;"></div>
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
