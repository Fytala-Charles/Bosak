# Promotion disposition — paired equality overrides

31 October 2026. Prepared technical disposition: retain both XdmValue.Equals(object) and GetHashCode() overrides together in the proposed 1.36.0 patch.

Private evidence must not become part of ordinary CLR value equality or hashing. The overrides preserve the original implementation for unmarked values and compare/hash ordinary fields for marked values. The existing inherited callable signatures and base virtual slots remain stable. This keeps default equality, hashing and hash-based collections consistent for the qualified fixtures.

The declared public reflection manifest changes explicitly: 78 exported types remain, while declared members rise from 1,199 to 1,201. The added declarations are Boolean Equals(System.Object) and Int32 GetHashCode() on XdmValue. MethodInfo.DeclaringType changes from System.ValueType to XdmValue. DeclaredOnly enumeration and declaration/module inspection can observe that change; it must be retained in promotion/release notes. The patch adds no public schema provider factory or component API.

This is the patch's technical recommendation. It does not record owner acceptance of C45, a public Schema SDK, final consumer compatibility, performance budgets or an API freeze. The proposed version is 1.36.0; the live source remains 1.35.0 until the patch is applied.

The patch's three implementation bodies match the qualified candidate's non-comment lines exactly. Production header/history and XML documentation changes, the minor version stamp, and 22 durable regression cases are included. Fresh preflight covers both affected test projects, versioned public metadata and the unchanged retained caller. Full 1.36.0 solution/conformance qualification must follow application; the sealed 1.35.0 candidate gates remain historical evidence for their original cohort.
