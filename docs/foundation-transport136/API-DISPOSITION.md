# Implemented equality-override disposition

31 October 2026. The paired technical disposition in the prepared promotion patch is now implemented in source version 1.36.0, following the owner's instruction to continue with application and final-version qualification.

XdmValue declares Equals(object) and GetHashCode() together. Unmarked values retain the previous ValueType implementation; marked values compare/hash ordinary fields without including private evidence. The seven foundation assemblies still export 78 types, with 1,201 declared public reflection members versus 1,199 in the prior product cohort. Only these two declarations are added. The inherited public signature lists and base virtual slots remain unchanged; XdmValue remains 48 bytes on the tested x64 runtime.

Reflection consumers can observe MethodInfo.DeclaringType changing from System.ValueType to XdmValue, and DeclaredOnly enumeration gains the two methods. This is a disclosed public-metadata change. Existing callable signatures remain available; the unchanged retained caller passes all 20 cases against the exact promoted seven-assembly cohort. No provider-facing schema factory or component API is exported.

The implementation disposition does not accept the broader C45 public provider manifest, a complete Schema SDK, full XSD 1.1, final consumer compatibility, owner performance budgets or API freeze. Those remain pending. Earlier proposal files remain sealed historical evidence, including their original unapplied/proposed flags. Version 1.36.0 is qualified in source here; no package is published by this step.
