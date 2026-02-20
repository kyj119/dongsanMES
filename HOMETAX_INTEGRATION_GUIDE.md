# 홈택스 전자세금계산서 연동 가이드

## 📋 목차
1. [개요](#개요)
2. [홈택스 API 방식](#홈택스-api-방식)
3. [ASP 파일 업로드 방식 (추천)](#asp-파일-업로드-방식-추천)
4. [구현 단계](#구현-단계)
5. [XML 포맷](#xml-포맷)
6. [보안 및 인증](#보안-및-인증)
7. [FAQ](#faq)

---

## 개요

홈택스 전자세금계산서 발행 방법은 크게 **3가지**입니다:

| 방식 | 난이도 | 비용 | 추천도 |
|------|--------|------|--------|
| **1. ASP 파일 업로드** | ⭐⭐ 중 | 무료 | ⭐⭐⭐⭐⭐ 강력 추천 |
| **2. 홈택스 OpenAPI** | ⭐⭐⭐⭐⭐ 매우 어려움 | 무료 | ⭐⭐ 비추천 |
| **3. 전문 솔루션 연동** | ⭐ 쉬움 | 월 3만원~ | ⭐⭐⭐⭐ 추천 (빠른 구현) |

---

## 홈택스 API 방식

### ❌ 문제점

1. **공인인증서 필요**: 법인 공인인증서 (비용: 연 11만원)
2. **복잡한 인증**: PKI 암호화, XML 전자서명
3. **불친절한 문서**: 공식 문서가 매우 복잡
4. **유지보수 어려움**: 인증서 갱신, API 변경 대응

### 📚 공식 문서

- **홈택스**: https://www.hometax.go.kr
- **국세청 전자세금계산서**: https://www.nts.go.kr
- **공인인증서**: https://www.signgate.com

---

## ASP 파일 업로드 방식 (추천) ⭐⭐⭐⭐⭐

### ✅ 장점

1. **무료**: 추가 비용 없음
2. **간단**: XML 파일만 생성하면 됨
3. **공인인증서 불필요**: 홈택스 로그인만으로 발행
4. **안정적**: 국세청 공식 지원 포맷

### 🔄 작업 흐름

```
1️⃣ MES 시스템에서 매출 확정
   ↓
2️⃣ 세금계산서 XML 파일 생성 (ASP 포맷)
   ↓
3️⃣ 파일 다운로드 (회계 담당자)
   ↓
4️⃣ 홈택스 접속 → 전자세금계산서 → 일괄발행
   ↓
5️⃣ XML 파일 업로드
   ↓
6️⃣ 발행 완료 → 승인번호 수동 입력 (또는 엑셀 일괄 업로드)
```

### 📝 ASP 포맷 구조

홈택스는 **ASP(Application Service Provider)** 표준 XML 포맷을 지원합니다.

---

## XML 포맷

### 1️⃣ 기본 구조

```xml
<?xml version="1.0" encoding="UTF-8"?>
<TaxInvoiceDocument xmlns="urn:kr:or:kec:standard:TaxInvoice:3.0">
  <TaxInvoice>
    <!-- 승인번호 (발행 후 자동 생성) -->
    <IssueID></IssueID>
    
    <!-- 작성일자 (YYYYMMDD) -->
    <IssueDateTime>20260211</IssueDateTime>
    
    <!-- 전자서명 (선택) -->
    <PurposeCode>영수</PurposeCode>
    
    <!-- 공급자 정보 -->
    <InvoicerParty>
      <PartyIdentification>
        <ID schemeID="사업자등록번호">123-45-67890</ID>
      </PartyIdentification>
      <PartyName>
        <Name>(주)동산인쇄</Name>
      </PartyName>
      <Contact>
        <Name>대표자명</Name>
        <Telephone>02-1234-5678</Telephone>
        <ElectronicMail>company@example.com</ElectronicMail>
      </Contact>
      <Address>
        <Line>서울시 강남구 테헤란로 123</Line>
      </Address>
      <PartyTaxScheme>
        <TaxScheme>
          <Name>업태</Name>
          <TaxTypeCode>종목</TaxTypeCode>
        </TaxScheme>
      </PartyTaxScheme>
    </InvoicerParty>
    
    <!-- 공급받는자 정보 -->
    <InvoiceeParty>
      <PartyIdentification>
        <ID schemeID="사업자등록번호">987-65-43210</ID>
      </PartyIdentification>
      <PartyName>
        <Name>(주)거래처명</Name>
      </PartyName>
      <Contact>
        <Name>대표자명</Name>
        <Telephone>02-9876-5432</Telephone>
        <ElectronicMail>client@example.com</ElectronicMail>
      </Contact>
      <Address>
        <Line>서울시 서초구 서초대로 456</Line>
      </Address>
      <PartyTaxScheme>
        <TaxScheme>
          <Name>업태</Name>
          <TaxTypeCode>종목</TaxTypeCode>
        </TaxScheme>
      </PartyTaxScheme>
    </InvoiceeParty>
    
    <!-- 공급 내역 -->
    <TaxInvoiceLine>
      <InvoicedQuantity>100</InvoicedQuantity>
      <Item>
        <Description>태극기 90x135</Description>
        <ClassifiedTaxCategory>
          <Percent>10</Percent>
        </ClassifiedTaxCategory>
      </Item>
      <Price>
        <PriceAmount>1000</PriceAmount>
      </Price>
      <TaxTotal>
        <TaxAmount>10000</TaxAmount>
      </TaxTotal>
      <LineExtensionAmount>100000</LineExtensionAmount>
    </TaxInvoiceLine>
    
    <!-- 합계 금액 -->
    <TaxTotal>
      <TaxAmount>10000</TaxAmount>
    </TaxTotal>
    <LegalMonetaryTotal>
      <TaxExclusiveAmount>100000</TaxExclusiveAmount>
      <TaxInclusiveAmount>110000</TaxInclusiveAmount>
    </LegalMonetaryTotal>
  </TaxInvoice>
</TaxInvoiceDocument>
```

### 2️⃣ 간소화된 버전 (홈택스 일괄발행용)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ETAXBILISSUE>
  <TAX_INVOICE>
    <!-- 작성일자 -->
    <WRITE_DT>20260211</WRITE_DT>
    
    <!-- 공급가액 -->
    <SUP_AMT>100000</SUP_AMT>
    
    <!-- 세액 -->
    <TAX_AMT>10000</TAX_AMT>
    
    <!-- 합계 -->
    <TOT_AMT>110000</TOT_AMT>
    
    <!-- 공급자 정보 -->
    <INV_PARTY>
      <BIZ_NO>123-45-67890</BIZ_NO>
      <COMPANY_NM>(주)동산인쇄</COMPANY_NM>
      <CEO_NM>홍길동</CEO_NM>
      <ADDR>서울시 강남구</ADDR>
      <BIZ_TYPE>제조업</BIZ_TYPE>
      <BIZ_CLASS>인쇄</BIZ_CLASS>
      <EMAIL>company@example.com</EMAIL>
    </INV_PARTY>
    
    <!-- 공급받는자 정보 -->
    <INVEE_PARTY>
      <BIZ_NO>987-65-43210</BIZ_NO>
      <COMPANY_NM>(주)거래처</COMPANY_NM>
      <CEO_NM>김철수</CEO_NM>
      <ADDR>서울시 서초구</ADDR>
      <BIZ_TYPE>도소매</BIZ_TYPE>
      <BIZ_CLASS>판매</BIZ_CLASS>
      <EMAIL>client@example.com</EMAIL>
    </INVEE_PARTY>
    
    <!-- 품목 -->
    <ITEM_LIST>
      <ITEM>
        <ITEM_NM>태극기 90x135</ITEM_NM>
        <SPEC>국기</SPEC>
        <QTY>100</QTY>
        <UNIT_AMT>1000</UNIT_AMT>
        <SUP_AMT>100000</SUP_AMT>
        <TAX_AMT>10000</TAX_AMT>
      </ITEM>
    </ITEM_LIST>
    
    <!-- 비고 -->
    <REMARK>2월분 매출</REMARK>
  </TAX_INVOICE>
</ETAXBILISSUE>
```

---

## 구현 단계

### Phase 1: XML 생성 기능 (1주)

#### 1. TaxInvoiceXmlGenerator 서비스 생성

```csharp
// Services/TaxInvoiceXmlGenerator.cs
using System.Xml.Linq;
using System.Text;
using MESSystem.Models;

namespace MESSystem.Services;

public class TaxInvoiceXmlGenerator
{
    /// <summary>
    /// 세금계산서를 홈택스 ASP 포맷 XML로 변환
    /// </summary>
    public string GenerateXml(TaxInvoice invoice)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("ETAXBILISSUE",
                new XElement("TAX_INVOICE",
                    // 작성일자
                    new XElement("WRITE_DT", invoice.IssueDate.ToString("yyyyMMdd")),
                    
                    // 공급가액
                    new XElement("SUP_AMT", invoice.SupplyAmount),
                    
                    // 세액
                    new XElement("TAX_AMT", invoice.TaxAmount),
                    
                    // 합계
                    new XElement("TOT_AMT", invoice.TotalAmount),
                    
                    // 공급자 정보
                    new XElement("INV_PARTY",
                        new XElement("BIZ_NO", invoice.SupplierBusinessNumber),
                        new XElement("COMPANY_NM", invoice.SupplierName),
                        new XElement("CEO_NM", invoice.SupplierCeoName),
                        new XElement("ADDR", invoice.SupplierAddress),
                        new XElement("BIZ_TYPE", invoice.SupplierBusinessType),
                        new XElement("BIZ_CLASS", invoice.SupplierBusinessItem),
                        new XElement("EMAIL", invoice.SupplierEmail)
                    ),
                    
                    // 공급받는자 정보
                    new XElement("INVEE_PARTY",
                        new XElement("BIZ_NO", invoice.BuyerBusinessNumber),
                        new XElement("COMPANY_NM", invoice.BuyerName),
                        new XElement("CEO_NM", invoice.BuyerCeoName),
                        new XElement("ADDR", invoice.BuyerAddress),
                        new XElement("BIZ_TYPE", invoice.BuyerBusinessType),
                        new XElement("BIZ_CLASS", invoice.BuyerBusinessItem),
                        new XElement("EMAIL", invoice.BuyerEmail)
                    ),
                    
                    // 품목 리스트
                    new XElement("ITEM_LIST",
                        invoice.Items.Select(item => 
                            new XElement("ITEM",
                                new XElement("ITEM_NM", item.ItemName),
                                new XElement("SPEC", item.Specification ?? ""),
                                new XElement("QTY", item.Quantity),
                                new XElement("UNIT_AMT", item.UnitPrice),
                                new XElement("SUP_AMT", item.SupplyAmount),
                                new XElement("TAX_AMT", item.TaxAmount)
                            )
                        )
                    ),
                    
                    // 비고
                    new XElement("REMARK", invoice.Memo ?? "")
                )
            )
        );
        
        return doc.ToString();
    }
    
    /// <summary>
    /// XML 파일로 저장
    /// </summary>
    public async Task<string> SaveToFileAsync(TaxInvoice invoice, string directory)
    {
        var xml = GenerateXml(invoice);
        var fileName = $"TAX_INVOICE_{invoice.IssueDate:yyyyMMdd}_{invoice.BuyerBusinessNumber}.xml";
        var filePath = Path.Combine(directory, fileName);
        
        await File.WriteAllTextAsync(filePath, xml, Encoding.UTF8);
        
        return filePath;
    }
    
    /// <summary>
    /// 여러 세금계산서를 하나의 XML로 묶기 (일괄 발행)
    /// </summary>
    public string GenerateBatchXml(List<TaxInvoice> invoices)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("ETAXBILISSUE",
                invoices.Select(invoice => 
                    new XElement("TAX_INVOICE",
                        // ... (위와 동일)
                    )
                )
            )
        );
        
        return doc.ToString();
    }
}
```

#### 2. appsettings.json에 설정 추가

```json
{
  "TaxInvoice": {
    "SupplierBusinessNumber": "123-45-67890",
    "SupplierName": "(주)동산인쇄",
    "SupplierCeoName": "홍길동",
    "SupplierAddress": "서울시 강남구 테헤란로 123",
    "SupplierBusinessType": "제조업",
    "SupplierBusinessItem": "인쇄",
    "SupplierEmail": "company@example.com",
    "XmlOutputPath": "C:\\TaxInvoice\\XML"
  }
}
```

---

### Phase 2: 홈택스 업로드 (수동)

#### 1. XML 다운로드 기능

```csharp
// Pages/ERP/TaxInvoices/Download.cshtml.cs
public class DownloadModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly TaxInvoiceXmlGenerator _xmlGenerator;
    
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var invoice = await _context.TaxInvoices
            .Include(t => t.Items)
            .Include(t => t.SalesClosing)
            .ThenInclude(s => s.Client)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (invoice == null)
            return NotFound();
        
        var xml = _xmlGenerator.GenerateXml(invoice);
        var fileName = $"세금계산서_{invoice.BuyerName}_{invoice.IssueDate:yyyyMMdd}.xml";
        var bytes = Encoding.UTF8.GetBytes(xml);
        
        return File(bytes, "application/xml", fileName);
    }
}
```

#### 2. 홈택스 업로드 절차

**담당자 매뉴얼:**

1. MES 시스템에서 세금계산서 XML 다운로드
2. 홈택스 접속 (https://www.hometax.go.kr)
3. 조회/발급 → 전자세금계산서 → 일괄발행
4. XML 파일 선택 업로드
5. 발행 완료 후 승인번호 확인
6. (선택) 승인번호 MES 시스템에 입력

---

### Phase 3: 승인번호 자동 조회 (선택, 고급)

홈택스에서 발행한 세금계산서의 승인번호를 자동으로 조회하려면:

#### 방법 1: 웹 스크래핑 (비추천)

- Selenium 등으로 홈택스 로그인 → 발행 내역 조회
- **문제점**: 홈택스 UI 변경시 오류, 로그인 보안 이슈

#### 방법 2: 홈택스 OpenAPI (매우 복잡)

- 공인인증서 필요
- PKI 암호화 구현
- **추천하지 않음**

#### 방법 3: 수동 입력 (현실적)

```csharp
// Pages/ERP/TaxInvoices/UpdateApproval.cshtml.cs
public class UpdateApprovalModel : PageModel
{
    [BindProperty]
    public int TaxInvoiceId { get; set; }
    
    [BindProperty]
    public string ApprovalNumber { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var invoice = await _context.TaxInvoices.FindAsync(TaxInvoiceId);
        
        if (invoice == null)
            return NotFound();
        
        invoice.ApprovalNumber = ApprovalNumber;
        invoice.Status = "발행완료";
        invoice.IssuedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        return RedirectToPage("./Index");
    }
}
```

---

## 보안 및 인증

### 1. XML 파일 보안

- 사업자등록번호 등 민감 정보 포함
- HTTPS 전송 필수
- 파일 다운로드 후 즉시 삭제 권장

### 2. 접근 권한

```csharp
// Pages/ERP/TaxInvoices/Index.cshtml.cs
[Authorize(Roles = "관리자,회계")]
public class IndexModel : PageModel
{
    // 회계 담당자만 접근 가능
}
```

---

## FAQ

### Q1: 공인인증서 없이 발행 가능한가요?

**A:** ASP 파일 업로드 방식을 사용하면 홈택스 로그인만으로 발행 가능합니다. 단, 홈택스에서 업로드 시 로그인 필요.

### Q2: 일괄 발행은 몇 건까지 가능한가요?

**A:** 홈택스는 1회 최대 **1,000건**까지 일괄 발행 가능합니다.

### Q3: 수정 세금계산서는 어떻게 발행하나요?

**A:** XML에 `<MODI_CODE>` 태그 추가:
- `1`: 기재사항 착오 정정
- `2`: 공급가액 착오 정정
- `3`: 환입
- `4`: 계약 해제

```xml
<TAX_INVOICE>
  <MODI_CODE>1</MODI_CODE>
  <ORIGINAL_ISS_ID>원본승인번호</ORIGINAL_ISS_ID>
  <!-- 나머지 내용 -->
</TAX_INVOICE>
```

### Q4: 역발행은 어떻게 하나요?

**A:** `<ISS_TYPE>02</ISS_TYPE>` 추가 (01: 정발행, 02: 역발행)

### Q5: 전자서명은 필수인가요?

**A:** ASP 파일 업로드 방식에서는 **불필요**합니다. 홈택스에서 업로드 시 자동으로 전자서명됩니다.

---

## 결론

### ✅ 추천 방식: ASP 파일 업로드

**장점:**
- 무료
- 공인인증서 불필요
- 구현 간단 (XML 생성만)
- 안정적

**단점:**
- 승인번호 수동 입력 (또는 일괄 입력)
- 완전 자동화는 어려움

**대안:**
- 빠른 구현이 필요하면: **바로빌/팝빌** (월 3만원)
- 완전 자동화가 필요하면: **바로빌/팝빌** (월 3만원)
- 비용 절감이 최우선이면: **ASP 파일 업로드** (무료)

---

## 다음 단계

1. ✅ XML 생성 서비스 구현
2. ✅ 다운로드 기능 추가
3. ⏳ 홈택스 업로드 매뉴얼 작성
4. ⏳ 승인번호 입력 화면 개발
5. ⏳ 테스트 환경 구축

---

**작성일**: 2026-02-11  
**작성자**: MES ERP 개발팀  
**버전**: 1.0
