# Usability Validation (SC-004)

Date: 2026-03-07
Validation method: scripted integration test
Command:
`dotnet test --filter "FullyQualifiedName~RedisKeysControllerSearchTests.SearchKeys_UsabilityValidation_AtLeastNinetyPercentExpirationClassificationAccuracy" --logger "console;verbosity=detailed"`

## Sample Set

- Total representative keys evaluated: 10
- Mixed populations included: persistent and expiring keys
- Validated output field: `hasExpiration`

## Results

- Correct classifications: 10
- Accuracy: 100.00%
- Success criterion (SC-004): >= 90%
- Outcome: PASS

## Raw Test Output Excerpt

`SC-004 usability validation: samples=10, correct=10, accuracy=100,00%`
