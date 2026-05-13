# DotNut.BLS12-381 — Roadmap to Production

Stan na dziś oznaczony jako ✅ (gotowe), ⚠️ (stub/niepełne), ❌ (brakuje).

---

## Aktualny stan

| Komponent | Stan | Uwagi |
|---|---|---|
| `Fp` — arytmetyka Montgomery, serializacja, CT | ✅ | |
| `Fp2`, `Fp6`, `Fp12` — arytmetyka wieży, Frobenius | ✅ | |
| `G1Affine` / `G1Projective` — scalar mul CT, subgroup check | ✅ | |
| `G2Affine` / `G2Projective` — scalar mul CT, subgroup check | ✅ | |
| `Scalar` — pełna arytmetyka Montgomery, serializacja, CT | ✅ | storage w Montgomery (jak Fp) |
| `Fp12.FinalExponentiation` — easy part + hard part (Beuchat) | ✅ | |
| `Fp12.CyclotomicSquare` — Algorithm 5.5.4 | ✅ | |
| `Fp12.CyclotomicExp` / `CyclotomicExpBlsX` | ✅ | |
| `Fp12.MulBy014` — sparse line eval | ✅ | |
| `Gt` — typ grupy docelowej | ✅ | Add/Negate/Double/Multiply(Scalar)/Generator |
| `MillerLoopResult` — typ wrappera | ✅ | |
| `G2Prepared` + `MultiMillerLoop` | ✅ | 68 współczynników, binarny Miller loop |
| Serializacja punktów (compressed/uncompressed) | ✅ | G1/G2 ZCash format, Fp/Fp2 sqrt |
| Hash-to-curve (RFC 9380) | ❌ | |
| BLS signatures (IETF draft) | ❌ | |
| Testy: EIP-2537 pairing vectors | ❌ | |
| Benchmarki | ❌ | |
| NuGet packaging | ❌ | |

---

## Faza 1 — Scalar arithmetic

Blokuje wszystko poniżej (hash-to-curve, BLS, Gt scalar mul).

- [x] `Scalar.Add(a, b)` — dodawanie mod r, CT
- [x] `Scalar.Sub(a, b)` — odejmowanie mod r, CT
- [x] `Scalar.Mul(a, b)` — mnożenie Montgomery mod r (4-limbowe), CT
- [x] `Scalar.Square(a)` — dedykowane SquareWide (upper-triangle + double + diagonal), 22 Mac vs 32
- [x] `Scalar.Negate(a)` — `r - a` z CT maską na zero
- [x] `Scalar.Invert(a)` — a^(r−2) mod r, square-and-multiply CT
- [x] `Scalar.ToBytesLittleEndian` / `FromBytesLittleEndian` / `TryFrom`
- [x] `Scalar.ToBytesBigEndian` / `FromBytesBigEndian` / `TryFrom`
- [x] `Scalar.FromBytesWide(ReadOnlySpan<byte> bytes64)` — (lo + hi·2²⁵⁶) mod r
- [x] `Scalar.IsZero`, `Scalar.Equal`, `==`, `!=`, `Equals`, `GetHashCode`

**Refaktor:** Scalar przechowuje elementy w formacie Montgomery (`a·R mod r`, R=2²⁵⁶) — identycznie jak `Fp`. Eliminuje podwójny MontReduce w `Mul`/`Square`. Konwersje: `FromCanonical` / `ToCanonical` (używane przez `GetBit`, serializację, `BigInteger`).

**Testy:**
- [ ] `ScalarTests` — Add/Sub/Mul/Invert (wektory z EIP-2537 scalar test suite)
- [ ] Roundtrip serializacja (LE/BE)

---

## Faza 2 — Optymalizacje Fp12 i pairing ✅

- [x] `Fp12.CyclotomicSquare` — Algorithm 5.5.4 z [eprint.iacr.org/2009/565](https://eprint.iacr.org/2009/565.pdf)
- [x] `Fp12.CyclotomicExp` / `CyclotomicExpBlsX` — używa `CyclotomicSquare`
- [x] `Fp12.FinalExponentiation` hard part — Beuchat et al. (Frobenius + cyclotomic exp)
- [x] `Fp12.MulBy014(c0, c1, c4)` — sparse multiply dla line evaluation
- [x] `Fp6.MulBy01`, `Fp6.MulBy1` — pomocnicze sparse mul
- [x] `Fp12.Square` — zoptymalizowana formuła Karatsuba (3 Fp6 mul zamiast 4)
- [x] `LineFunction` w `Bls12Pairing` — użyje `MulBy014`
- [x] Fix: `AteLoopSize` / `BLS_X` — błąd znaku w `BigInteger.Parse` (brakujące `"0"` przed hex)

**Testy:** ✅ wszystkie przeszły

---

## Faza 3 — Pairing API ✅

- [x] `MillerLoopResult` — struct wrappujący `Fp12`, z `FinalExponentiation() -> Gt` i `Default`
- [x] `Gt` — struct wrappujący `Fp12` po final exp:
  - `Identity`, `Generator` (hardkodowany z zkcrypto)
  - `Double()` → `Fp12.Square`
  - `Negate()` → `Fp12.Conjugate` (element unitary)
  - `Add(Gt, Gt)`, `Sub(Gt, Gt)`, `Multiply(Gt, Scalar)` — double-and-add MSB→LSB
  - `Equal(Gt, Gt)`, `ConditionalSelect`
- [x] `G2Prepared` — prekalkulacja 68 współczynników linii (Algorithm 26/27, binarny BLS_X>>1)
- [x] `MultiMillerLoop(IEnumerable<(G1Affine, G2Prepared)>) -> MillerLoopResult`
- [x] `Bls12Pairing.Pair` zwraca `Gt` zamiast `Fp12`
- [x] `Bls12Pairing.MillerLoop` zwraca `MillerLoopResult`
- [x] Przełączenie Miller loop: NAF → binarny BLS_X>>1 (=0x6900800000008000, dokładnie 68 kroków)

**Testy:** ✅ 108/108

---

## Faza 4 — Serializacja punktów (format ZCash) ✅

Standard: [ZCash serialization spec](https://github.com/zcash/librustzcash/blob/master/pairing/src/bls12_381/README.md), identyczny z Ethereum EIP-2537 dla uncompressed.

**G1 (48/96 bajtów):**
- [x] `G1Affine.ToCompressed()` — ustawia bit C (MSB), bit S (znak y), bit I (infinity)
- [x] `G1Affine.TryFromCompressed(ReadOnlySpan<byte>, out G1Affine)` — odtwarza y z x (Shank sqrt w Fp)
- [x] `G1Affine.ToUncompressed()`
- [x] `G1Affine.TryFromUncompressed(ReadOnlySpan<byte>, out G1Affine)`
- [x] `Fp.TrySqrt` / `Fp.LexicographicallyLargest` / `Fp.PowVartime`

**G2 (96/192 bajtów):**
- [x] `G2Affine.ToCompressed()` — x zakodowane jako `x.C1 || x.C0` (c1 first)
- [x] `G2Affine.TryFromCompressed(ReadOnlySpan<byte>, out G2Affine)` — sqrt w Fp2 (Algorithm 9)
- [x] `G2Affine.ToUncompressed()`
- [x] `G2Affine.TryFromUncompressed(ReadOnlySpan<byte>, out G2Affine)`
- [x] `Fp2.TrySqrt` / `Fp2.LexicographicallyLargest` / `Fp2.PowVartime`

**Testy:** ✅ 22 nowych testów (round-trip G1/G2, flagi, odrzucanie złych bajtów)
- [x] Round-trip compressed/uncompressed dla G1, G2 (generator + infinity + 3G)
- [x] Odrzucanie nieprawidłowych encodingów (zła długość, złe flagi)
- [ ] EIP-2537 pairing test vectors (input G1/G2 w formacie EIP → wynik Gt)

---

## Faza 5 — Cofactor clearing ✅

Potrzebne do hash-to-curve (bezpieczeństwo: musi trafiać w subgrupę porządku r).

- [x] `G1Projective.ClearCofactor()` — [h1]P = P + [BLS_X]P, 64-bitowy double-and-add (h1 = 1+BLS_X)
- [x] `G2Projective.ClearCofactor()` — algorytm Wahby-Boneh 2019: psi^2(2P)+(x^2-x-1)P+(x-1)*psi(P), 2x64-bit + 2x psi
- [x] `G2Projective.Psi` / `Psi2` — endomorphism untwist-Frobenius-twist z PSI_X=xi^(-(p-1)/3), PSI_Y=xi^(-(p-1)/2)
- [x] `G1Projective.MulByBLSX` — pomocnicze 64-bitowe mnozenie przez BLS_X
- [x] G2 subgroup check przez endomorphism — szybkie psi(P)+[BLS_X]P=O zamiast *r
- [ ] G1 subgroup check przez endomorphism (GLV-based, wymaga endomorphismu phi = cube-root-of-unity; TODO)

**Testy:** 19 nowych testow (CofactorTests.cs)
- [x] `ClearCofactor` na G, [2]G, [4]G dla G1 i G2 -> punkt w subgrupie
- [x] psi jest homomorfizmem: psi(P+Q)=psi(P)+psi(Q)
- [x] psi^2 = psi(psi(.)) (Psi2 zgodny z Psi(Psi(.)))
- [x] Szybki G2 subgroup check vs pelny *r (dla G i [2]G)

---

## Faza 6 — Hash-to-curve (RFC 9380)

Referencja: [RFC 9380](https://www.rfc-editor.org/rfc/rfc9380), [hash_to_curve Rust](https://github.com/zkcrypto/bls12_381/tree/main/src/hash_to_curve).

- [ ] `ExpandMessageXmd(byte[] msg, byte[] dst, int lenInBytes) -> byte[]` — HMAC-SHA256 based (RFC 9380 §5.3.1)
- [ ] `HashToField(byte[] msg, byte[] dst, int count) -> Fp[]` — dla G1 (RFC 9380 §5.2)
- [ ] `HashToFieldFp2(byte[] msg, byte[] dst, int count) -> Fp2[]` — dla G2
- [ ] `Fp.SqrtRatio(u, v) -> (bool, Fp)` — constant-time, potrzebne dla SWU (RFC 9380 §4)
- [ ] `MapToG1(Fp u) -> G1Affine` — simplified SWU dla E' + 3-isogenia do E₁ (RFC 9380 §6.6.2 + Appendix G.1)
- [ ] `MapToG2(Fp2 u) -> G2Affine` — simplified SWU dla E'₂ + isogenia do E₂ (RFC 9380 §6.6.3 + Appendix G.2)
- [ ] `HashToG1(byte[] msg, byte[] dst) -> G1Affine` — `hash_to_field` → 2× `MapToG1` → add → `ClearCofactor`
- [ ] `HashToG2(byte[] msg, byte[] dst) -> G2Affine` — analogicznie

**Testy:**
- [ ] RFC 9380 Appendix J.9 — test vectors dla `BLS12381G1_XMD:SHA-256_SSWU_RO_` (16 przypadków)
- [ ] RFC 9380 Appendix J.10 — test vectors dla `BLS12381G2_XMD:SHA-256_SSWU_RO_` (16 przypadków)

---

## Faza 7 — BLS Signatures

Standard: [IETF draft-irtf-cfrg-bls-signature-05](https://datatracker.ietf.org/doc/draft-irtf-cfrg-bls-signature/), ciphersuite `BLS_SIG_BLS12381G2_XMD:SHA-256_SSWU_RO_NUL_` (PK w G1, sygnatury w G2).

```
DotNut.BLS12-381/BLS/   ← istniejący pusty folder
```

- [ ] `BlsPrivateKey` — wraps `Scalar`, z `Generate(ReadOnlySpan<byte> ikm)` per §2.3
- [ ] `BlsPublicKey` — wraps `G1Affine`, z `FromPrivateKey(BlsPrivateKey)`
- [ ] `BlsSignature` — wraps `G2Affine`
- [ ] `BlsScheme.Sign(BlsPrivateKey sk, byte[] msg) -> BlsSignature`
- [ ] `BlsScheme.Verify(BlsPublicKey pk, byte[] msg, BlsSignature sig) -> bool`
- [ ] `BlsScheme.Aggregate(IEnumerable<BlsSignature> sigs) -> BlsSignature` — suma w G2
- [ ] `BlsScheme.AggregateVerify(IEnumerable<(BlsPublicKey, byte[])> pairs, BlsSignature sig) -> bool` — używa `MultiMillerLoop`
- [ ] `BlsScheme.FastAggregateVerify(IEnumerable<BlsPublicKey> pks, byte[] msg, BlsSignature sig) -> bool` — suma PK w G1
- [ ] `BlsScheme.PopProve(BlsPrivateKey sk) -> BlsSignature` — proof of possession
- [ ] `BlsScheme.PopVerify(BlsPublicKey pk, BlsSignature pop) -> bool`
- [ ] `Eip2333.DeriveChildSk(BlsPrivateKey parent, uint index) -> BlsPrivateKey` — hierarchiczne wyprowadzanie kluczy (EIP-2333)
- [ ] `Eip2333.DeriveChildPk(BlsPublicKey parent, uint index) -> BlsPublicKey`

**Testy:**
- [ ] IETF BLS test vectors (sign/verify/aggregate) — plik `draft-irtf-cfrg-bls-signature` appendix
- [ ] EIP-2537 pairing test vectors (wyniki e(G1, G2) z precomputed expected)
- [ ] Ethereum consensus spec test vectors dla BLS operacji

---

## Faza 8 — Zabezpieczenia i edge cases

- [ ] `Fp.Invert` — zamień `throw DivideByZeroException` na zwrot zera (constant-time, jak Rust `CtOption`) lub `TryInvert`
- [ ] `G1Affine.IsInSubgroup` / `G2Affine.IsInSubgroup` — zastąp kosztowną `×r` algorytmem endomorphism (patrz Faza 5)
- [ ] Weryfikacja że `G2Affine.IsInSubgroup` jest wywoływane przy deserializacji
- [ ] Ochrona przed timing attacks w `BlsScheme.Verify` (nie wychodź wcześniej z pętli weryfikacji)
- [ ] Dodaj `SecureRandom` wrapper (używa `RandomNumberGenerator.Fill`)

---

## Faza 9 — Testy regresyjne i fuzz

- [ ] `DotNut.BLS12-381.Tests/Arithmetic/` — testy arytmetyki Scalar (istniejący pusty folder)
- [ ] Testy serializacji dla wszystkich typów (round-trip, invalid input rejection)
- [ ] Property-based tests dla Scalar/Fp przez xunit + FsCheck lub AutoFixture
- [ ] Fuzz harness dla `TryFromCompressed` i `BlsScheme.Verify` (SharpFuzz lub własny AFL adapter)
- [ ] Benchmark baseline przed i po optymalizacjach fazy 2

---

## Faza 10 — Wydajność (BenchmarkDotNet)

Projekt `DotNut.BLS12-381.Benchmarks`:
- [ ] `Fp.Multiply` / `Fp.Square`
- [ ] `Fp12.FinalExponentiation` (przed i po fazie 2)
- [ ] `Bls12Pairing.Pair` (end-to-end)
- [ ] `MultiMillerLoop` (2, 4, 8 par)
- [ ] `G1.ScalarMultiply` / `G2.ScalarMultiply`
- [ ] `HashToG1` / `HashToG2`
- [ ] `BlsScheme.Sign` / `BlsScheme.Verify`
- [ ] `BlsScheme.AggregateVerify` (n=10, 100, 1000)

Punkty odniesienia: `bls12_381` (Rust), `mcl` (C++), `herumi/bls` (Go).

---

## Faza 11 — Packaging

- [ ] NuGet metadata w csproj (PackageId, Version, Authors, Description, License=Apache-2.0, RepositoryUrl)
- [ ] `<GenerateDocumentationFile>true</GenerateDocumentationFile>` + XML docs na całym publicznym API
- [ ] Multi-targeting: `net8.0;net9.0`
- [ ] Strong name signing (opcjonalnie, potrzebne dla niektórych enterprise środowisk)
- [ ] `SECURITY.md` — zasady zgłaszania podatności, policy
- [ ] `CHANGELOG.md`
- [ ] CI: GitHub Actions — build + test na linux-x64 i win-x64, publikacja NuGet na tag

---

## Kolejność priorytetów

```
Faza 1 (Scalar)
    → Faza 2 (Fp12 optymalizacje)
        → Faza 3 (Pairing API / Gt)
    → Faza 4 (Serializacja) ← niezależna od Fazy 2/3
    → Faza 5 (Cofactor clearing)
        → Faza 6 (Hash-to-curve)
            → Faza 7 (BLS Signatures)  ← wymaga wszystkiego powyżej
→ Faza 8 (Security) — równolegle z powyższymi
→ Fazy 9-11 — po kompletności API
```

---

## Zewnętrzne zestawy wektorów testowych

| Zestaw | URL | Używany w |
|---|---|---|
| EIP-2537 G1 add/mul | `eips.ethereum.org/assets/eip-2537/` | G1Tests ✅ |
| EIP-2537 G2 add/mul | jw. | G2Tests ✅ |
| EIP-2537 pairing | jw. | Faza 4 (serializacja) |
| RFC 9380 hash-to-g1/g2 | `rfc-editor.org/rfc/rfc9380` Appendix J | Faza 6 |
| IETF BLS signatures | `datatracker.ietf.org/doc/draft-irtf-cfrg-bls-signature` | Faza 7 |
| Ethereum consensus BLS | `github.com/ethereum/consensus-spec-tests` | Faza 7 |
