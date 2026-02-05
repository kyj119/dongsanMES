// 주문서 수정 폼 JavaScript

document.addEventListener('DOMContentLoaded', function() {
    initializeShippingMethodToggle();
    initializeItemCheckboxes();
    initializeProductSearch();
});

// 출고방법 선택 시 필드 토글
function initializeShippingMethodToggle() {
    const shippingMethodSelect = document.getElementById('shippingMethod');
    const paymentMethodGroup = document.getElementById('paymentMethodGroup');
    const shippingTimeGroup = document.getElementById('shippingTimeGroup');
    const paymentRequired = document.getElementById('paymentRequired');
    const timeRequired = document.getElementById('timeRequired');

    // 초기 상태 설정
    updateFieldsVisibility();

    shippingMethodSelect.addEventListener('change', function() {
        updateFieldsVisibility();
    });

    function updateFieldsVisibility() {
        const method = shippingMethodSelect.value;
        
        // 결제방법 필드 표시/숨김
        if (['대신택배', '대신화물', '한진택배', '퀵', '용차'].includes(method)) {
            paymentMethodGroup.style.display = 'block';
            paymentRequired.style.display = 'inline';
        } else {
            paymentMethodGroup.style.display = 'none';
            paymentRequired.style.display = 'none';
        }

        // 출고시간 필드 표시/숨김
        if (['퀵', '용차', '방문수령', '직접배송'].includes(method)) {
            shippingTimeGroup.style.display = 'block';
            timeRequired.style.display = 'inline';
        } else {
            shippingTimeGroup.style.display = 'none';
            timeRequired.style.display = 'none';
        }
    }
}

// 품목 체크박스 초기화
function initializeItemCheckboxes() {
    const selectAllCheckbox = document.getElementById('selectAll');
    const deleteSelectedBtn = document.getElementById('deleteSelectedBtn');
    
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            const checkboxes = document.querySelectorAll('.item-checkbox');
            checkboxes.forEach(cb => cb.checked = this.checked);
            updateDeleteButtonState();
        });
    }

    // 개별 체크박스 이벤트
    document.addEventListener('change', function(e) {
        if (e.target.classList.contains('item-checkbox')) {
            updateDeleteButtonState();
            updateSelectAllState();
        }
    });

    // 선택 삭제 버튼
    if (deleteSelectedBtn) {
        deleteSelectedBtn.addEventListener('click', function() {
            const checkedBoxes = document.querySelectorAll('.item-checkbox:checked');
            if (checkedBoxes.length === 0) {
                alert('삭제할 품목을 선택해주세요.');
                return;
            }

            if (confirm(`선택한 ${checkedBoxes.length}개 품목을 삭제하시겠습니까?`)) {
                checkedBoxes.forEach(cb => {
                    const row = cb.closest('tr');
                    row.remove();
                });
                reindexRows();
                updateDeleteButtonState();
            }
        });
    }
}

// 삭제 버튼 활성화/비활성화
function updateDeleteButtonState() {
    const deleteBtn = document.getElementById('deleteSelectedBtn');
    const checkedBoxes = document.querySelectorAll('.item-checkbox:checked');
    
    if (deleteBtn) {
        deleteBtn.disabled = checkedBoxes.length === 0;
        if (checkedBoxes.length > 0) {
            deleteBtn.innerHTML = `<i class="bi bi-trash"></i> 선택 삭제 (${checkedBoxes.length})`;
        } else {
            deleteBtn.innerHTML = '<i class="bi bi-trash"></i> 선택 삭제';
        }
    }
}

// 전체 선택 체크박스 상태 업데이트
function updateSelectAllState() {
    const selectAllCheckbox = document.getElementById('selectAll');
    const checkboxes = document.querySelectorAll('.item-checkbox');
    const checkedCount = document.querySelectorAll('.item-checkbox:checked').length;
    
    if (selectAllCheckbox) {
        selectAllCheckbox.checked = checkboxes.length > 0 && checkedCount === checkboxes.length;
        selectAllCheckbox.indeterminate = checkedCount > 0 && checkedCount < checkboxes.length;
    }
}

// 행 번호 재정렬
function reindexRows() {
    const rows = document.querySelectorAll('.item-row');
    rows.forEach((row, index) => {
        row.querySelector('.row-number').textContent = index + 1;
        
        // name 속성 재정렬
        row.querySelectorAll('input, select, textarea').forEach(input => {
            const name = input.getAttribute('name');
            if (name && name.includes('[')) {
                const newName = name.replace(/\[\d+\]/, `[${index}]`);
                input.setAttribute('name', newName);
            }
        });
    });
}

// 품목 검색 초기화
let productSearchTimeout = null;

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
}

// HTML 이스케이프 (XSS 방지)
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// 폼 제출 전 검증
document.getElementById('orderForm')?.addEventListener('submit', function(e) {
    const rows = document.querySelectorAll('.item-row');
    
    if (rows.length === 0) {
        alert('최소 1개 이상의 품목이 필요합니다.');
        e.preventDefault();
        return false;
    }

    // 제출 버튼 비활성화 (중복 제출 방지)
    const submitBtn = this.querySelector('button[type="submit"]');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>저장 중...';
});
