namespace HWAY.Payroll.Services.Calculations;

public static class DefaultPayrollCalculations
{
    public const string CppName = "CRA_CPP";
    public const string EiName = "CRA_EI";
    public const string TaxName = "CRA_INCOME_TAX";

    public const string CppCalculation = """
{
  "source": "CRA T4127 Payroll Deductions Formulas, 2026",
  "periods": [
    {
      "effectiveFrom": "2026-01-01",
      "ympe": 74600.00,
      "yampe": 85000.00,
      "basicExemption": 3500.00,
      "totalRate": 0.0595,
      "baseRate": 0.0495,
      "additionalRate": 0.0100,
      "cpp2Rate": 0.0400,
      "maximumContribution": 4230.45,
      "maximumBaseContribution": 3519.45,
      "maximumCpp2Contribution": 416.00,
      "monthsProrated": 12
    }
  ]
}
""";

    public const string EiCalculation = """
{
  "source": "CRA T4127 Payroll Deductions Formulas, 2026",
  "periods": [
    {
      "effectiveFrom": "2026-01-01",
      "maximumInsurableEarnings": 68900.00,
      "employeeRate": 0.0163,
      "maximumEmployeePremium": 1123.07
    }
  ]
}
""";

    public const string TaxCalculation = """
{
  "source": "CRA T4127 Payroll Deductions Formulas, 122nd Edition effective 2026-01-01 and 123rd Edition effective 2026-07-01",
  "defaultProvince": "ON",
  "periods": [
    {
      "effectiveFrom": "2026-01-01",
      "federal": {
        "brackets": [
          { "threshold": 0.00, "rate": 0.1400, "constant": 0.00 },
          { "threshold": 58523.00, "rate": 0.2050, "constant": 3804.00 },
          { "threshold": 117045.00, "rate": 0.2600, "constant": 10241.00 },
          { "threshold": 181440.00, "rate": 0.2900, "constant": 15685.00 },
          { "threshold": 258482.00, "rate": 0.3300, "constant": 26024.00 }
        ],
        "basicPersonalAmountMaximum": 16452.00,
        "basicPersonalAmountMinimum": 14829.00,
        "basicPersonalAmountPhaseoutStart": 181440.00,
        "basicPersonalAmountPhaseoutEnd": 258482.00,
        "canadaEmploymentAmount": 1501.00
      },
      "provinces": {
        "AB": {
          "basicPersonalAmount": 22769.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0800, "constant": 0.00 },
            { "threshold": 61200.00, "rate": 0.1000, "constant": 1224.00 },
            { "threshold": 154259.00, "rate": 0.1200, "constant": 4309.00 },
            { "threshold": 185111.00, "rate": 0.1300, "constant": 6160.00 },
            { "threshold": 246813.00, "rate": 0.1400, "constant": 8628.00 },
            { "threshold": 370220.00, "rate": 0.1500, "constant": 12331.00 }
          ]
        },
        "BC": {
          "basicPersonalAmount": 13216.00,
          "taxReductionBasicAmount": 575.00,
          "taxReductionStart": 25570.00,
          "taxReductionEnd": 41722.00,
          "taxReductionRate": 0.0356,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0506, "constant": 0.00 },
            { "threshold": 50363.00, "rate": 0.0770, "constant": 1330.00 },
            { "threshold": 100728.00, "rate": 0.1050, "constant": 4150.00 },
            { "threshold": 115648.00, "rate": 0.1229, "constant": 6220.00 },
            { "threshold": 140430.00, "rate": 0.1470, "constant": 9604.00 },
            { "threshold": 190405.00, "rate": 0.1680, "constant": 13603.00 },
            { "threshold": 265545.00, "rate": 0.2050, "constant": 23428.00 }
          ]
        },
        "MB": {
          "basicPersonalAmount": 15780.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.1080, "constant": 0.00 },
            { "threshold": 47000.00, "rate": 0.1275, "constant": 917.00 },
            { "threshold": 100000.00, "rate": 0.1740, "constant": 5567.00 }
          ]
        },
        "NB": {
          "basicPersonalAmount": 13664.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0940, "constant": 0.00 },
            { "threshold": 52333.00, "rate": 0.1400, "constant": 2407.00 },
            { "threshold": 104666.00, "rate": 0.1600, "constant": 4501.00 },
            { "threshold": 193861.00, "rate": 0.1950, "constant": 11286.00 }
          ]
        },
        "NL": {
          "basicPersonalAmount": 11188.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0870, "constant": 0.00 },
            { "threshold": 44678.00, "rate": 0.1450, "constant": 2591.00 },
            { "threshold": 89354.00, "rate": 0.1580, "constant": 3753.00 },
            { "threshold": 159528.00, "rate": 0.1780, "constant": 6943.00 },
            { "threshold": 223340.00, "rate": 0.1980, "constant": 11410.00 },
            { "threshold": 285319.00, "rate": 0.2080, "constant": 14263.00 },
            { "threshold": 570638.00, "rate": 0.2130, "constant": 17117.00 },
            { "threshold": 1141275.00, "rate": 0.2180, "constant": 22823.00 }
          ]
        },
        "NS": {
          "basicPersonalAmount": 11932.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0879, "constant": 0.00 },
            { "threshold": 30995.00, "rate": 0.1495, "constant": 1909.00 },
            { "threshold": 61991.00, "rate": 0.1667, "constant": 2976.00 },
            { "threshold": 97417.00, "rate": 0.1750, "constant": 3784.00 },
            { "threshold": 157124.00, "rate": 0.2100, "constant": 9283.00 }
          ]
        },
        "NT": {
          "basicPersonalAmount": 18198.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0590, "constant": 0.00 },
            { "threshold": 53003.00, "rate": 0.0860, "constant": 1431.00 },
            { "threshold": 106009.00, "rate": 0.1220, "constant": 5247.00 },
            { "threshold": 172346.00, "rate": 0.1405, "constant": 8436.00 }
          ]
        },
        "NU": {
          "basicPersonalAmount": 19659.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0400, "constant": 0.00 },
            { "threshold": 55801.00, "rate": 0.0700, "constant": 1674.00 },
            { "threshold": 111602.00, "rate": 0.0900, "constant": 3906.00 },
            { "threshold": 181439.00, "rate": 0.1150, "constant": 8442.00 }
          ]
        },
        "ON": {
          "basicPersonalAmount": 12989.00,
          "taxReductionBasicAmount": 300.00,
          "surtaxFirstThreshold": 5818.00,
          "surtaxFirstRate": 0.20,
          "surtaxSecondThreshold": 7446.00,
          "surtaxSecondRate": 0.36,
          "healthPremium": [
            { "start": 20000.00, "end": 36000.00, "base": 0.00, "rate": 0.06, "maximum": 300.00 },
            { "start": 36000.00, "end": 48000.00, "base": 300.00, "rate": 0.06, "maximum": 450.00 },
            { "start": 48000.00, "end": 72000.00, "base": 450.00, "rate": 0.25, "maximum": 600.00 },
            { "start": 72000.00, "end": 200000.00, "base": 600.00, "rate": 0.25, "maximum": 750.00 },
            { "start": 200000.00, "end": null, "base": 750.00, "rate": 0.25, "maximum": 900.00 }
          ],
          "brackets": [
            { "threshold": 0.00, "rate": 0.0505, "constant": 0.00 },
            { "threshold": 53891.00, "rate": 0.0915, "constant": 2210.00 },
            { "threshold": 107785.00, "rate": 0.1116, "constant": 4376.00 },
            { "threshold": 150000.00, "rate": 0.1216, "constant": 5876.00 },
            { "threshold": 220000.00, "rate": 0.1316, "constant": 8076.00 }
          ]
        },
        "PE": {
          "basicPersonalAmount": 15000.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0950, "constant": 0.00 },
            { "threshold": 33928.00, "rate": 0.1347, "constant": 1347.00 },
            { "threshold": 65820.00, "rate": 0.1660, "constant": 3407.00 },
            { "threshold": 106890.00, "rate": 0.1762, "constant": 4497.00 },
            { "threshold": 142520.00, "rate": 0.1900, "constant": 6464.00 }
          ]
        },
        "SK": {
          "basicPersonalAmount": 20381.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.1050, "constant": 0.00 },
            { "threshold": 54532.00, "rate": 0.1250, "constant": 1091.00 },
            { "threshold": 155805.00, "rate": 0.1450, "constant": 4207.00 }
          ]
        },
        "YT": {
          "basicPersonalAmount": 16452.00,
          "territorialCanadaEmploymentAmount": true,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0640, "constant": 0.00 },
            { "threshold": 58523.00, "rate": 0.0900, "constant": 1522.00 },
            { "threshold": 117045.00, "rate": 0.1090, "constant": 3745.00 },
            { "threshold": 181440.00, "rate": 0.1280, "constant": 7193.00 },
            { "threshold": 500000.00, "rate": 0.1500, "constant": 18193.00 }
          ]
        }
      }
    },
    {
      "effectiveFrom": "2026-07-01",
      "federal": {
        "brackets": [
          { "threshold": 0.00, "rate": 0.1400, "constant": 0.00 },
          { "threshold": 58523.00, "rate": 0.2050, "constant": 3804.00 },
          { "threshold": 117045.00, "rate": 0.2600, "constant": 10241.00 },
          { "threshold": 181440.00, "rate": 0.2900, "constant": 15685.00 },
          { "threshold": 258482.00, "rate": 0.3300, "constant": 26024.00 }
        ],
        "basicPersonalAmountMaximum": 16452.00,
        "basicPersonalAmountMinimum": 14829.00,
        "basicPersonalAmountPhaseoutStart": 181440.00,
        "basicPersonalAmountPhaseoutEnd": 258482.00,
        "canadaEmploymentAmount": 1501.00
      },
      "provinces": {
        "AB": {
          "basicPersonalAmount": 22769.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0800, "constant": 0.00 },
            { "threshold": 61200.00, "rate": 0.1000, "constant": 1224.00 },
            { "threshold": 154259.00, "rate": 0.1200, "constant": 4309.00 },
            { "threshold": 185111.00, "rate": 0.1300, "constant": 6160.00 },
            { "threshold": 246813.00, "rate": 0.1400, "constant": 8628.00 },
            { "threshold": 370220.00, "rate": 0.1500, "constant": 12331.00 }
          ]
        },
        "BC": {
          "basicPersonalAmount": 13216.00,
          "taxReductionBasicAmount": 805.00,
          "taxReductionStart": 25570.00,
          "taxReductionEnd": 44952.00,
          "taxReductionRate": 0.0356,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0614, "constant": 0.00 },
            { "threshold": 50363.00, "rate": 0.0770, "constant": 786.00 },
            { "threshold": 100728.00, "rate": 0.1050, "constant": 3606.00 },
            { "threshold": 115648.00, "rate": 0.1229, "constant": 5676.00 },
            { "threshold": 140430.00, "rate": 0.1470, "constant": 9061.00 },
            { "threshold": 190405.00, "rate": 0.1680, "constant": 13059.00 },
            { "threshold": 265545.00, "rate": 0.2050, "constant": 22884.00 }
          ]
        },
        "MB": {
          "basicPersonalAmount": 15780.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.1080, "constant": 0.00 },
            { "threshold": 47000.00, "rate": 0.1275, "constant": 917.00 },
            { "threshold": 100000.00, "rate": 0.1740, "constant": 5567.00 }
          ]
        },
        "NB": {
          "basicPersonalAmount": 13664.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0940, "constant": 0.00 },
            { "threshold": 52333.00, "rate": 0.1400, "constant": 2407.00 },
            { "threshold": 104666.00, "rate": 0.1600, "constant": 4501.00 },
            { "threshold": 193861.00, "rate": 0.1950, "constant": 11286.00 }
          ]
        },
        "NL": {
          "basicPersonalAmount": 15000.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0870, "constant": 0.00 },
            { "threshold": 44678.00, "rate": 0.1450, "constant": 2591.00 },
            { "threshold": 89354.00, "rate": 0.1580, "constant": 3753.00 },
            { "threshold": 159528.00, "rate": 0.1780, "constant": 6943.00 },
            { "threshold": 223340.00, "rate": 0.1980, "constant": 11410.00 },
            { "threshold": 285319.00, "rate": 0.2080, "constant": 14263.00 },
            { "threshold": 570638.00, "rate": 0.2130, "constant": 17117.00 },
            { "threshold": 1141275.00, "rate": 0.2180, "constant": 22823.00 }
          ]
        },
        "NS": {
          "basicPersonalAmount": 11932.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0879, "constant": 0.00 },
            { "threshold": 30995.00, "rate": 0.1495, "constant": 1909.00 },
            { "threshold": 61991.00, "rate": 0.1667, "constant": 2976.00 },
            { "threshold": 97417.00, "rate": 0.1750, "constant": 3784.00 },
            { "threshold": 157124.00, "rate": 0.2100, "constant": 9283.00 }
          ]
        },
        "NT": {
          "basicPersonalAmount": 18198.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0590, "constant": 0.00 },
            { "threshold": 53003.00, "rate": 0.0860, "constant": 1431.00 },
            { "threshold": 106009.00, "rate": 0.1220, "constant": 5247.00 },
            { "threshold": 172346.00, "rate": 0.1405, "constant": 8436.00 }
          ]
        },
        "NU": {
          "basicPersonalAmount": 19659.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0400, "constant": 0.00 },
            { "threshold": 55801.00, "rate": 0.0700, "constant": 1674.00 },
            { "threshold": 111602.00, "rate": 0.0900, "constant": 3906.00 },
            { "threshold": 181439.00, "rate": 0.1150, "constant": 8442.00 }
          ]
        },
        "ON": {
          "basicPersonalAmount": 12989.00,
          "taxReductionBasicAmount": 300.00,
          "surtaxFirstThreshold": 5818.00,
          "surtaxFirstRate": 0.20,
          "surtaxSecondThreshold": 7446.00,
          "surtaxSecondRate": 0.36,
          "healthPremium": [
            { "start": 20000.00, "end": 36000.00, "base": 0.00, "rate": 0.06, "maximum": 300.00 },
            { "start": 36000.00, "end": 48000.00, "base": 300.00, "rate": 0.06, "maximum": 450.00 },
            { "start": 48000.00, "end": 72000.00, "base": 450.00, "rate": 0.25, "maximum": 600.00 },
            { "start": 72000.00, "end": 200000.00, "base": 600.00, "rate": 0.25, "maximum": 750.00 },
            { "start": 200000.00, "end": null, "base": 750.00, "rate": 0.25, "maximum": 900.00 }
          ],
          "brackets": [
            { "threshold": 0.00, "rate": 0.0505, "constant": 0.00 },
            { "threshold": 53891.00, "rate": 0.0915, "constant": 2210.00 },
            { "threshold": 107785.00, "rate": 0.1116, "constant": 4376.00 },
            { "threshold": 150000.00, "rate": 0.1216, "constant": 5876.00 },
            { "threshold": 220000.00, "rate": 0.1316, "constant": 8076.00 }
          ]
        },
        "PE": {
          "basicPersonalAmount": 15000.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0950, "constant": 0.00 },
            { "threshold": 33928.00, "rate": 0.1347, "constant": 1347.00 },
            { "threshold": 65820.00, "rate": 0.1660, "constant": 3407.00 },
            { "threshold": 106890.00, "rate": 0.1762, "constant": 4497.00 },
            { "threshold": 142520.00, "rate": 0.1900, "constant": 6464.00 },
            { "threshold": 200000.00, "rate": 0.2100, "constant": 10464.00 }
          ]
        },
        "SK": {
          "basicPersonalAmount": 20381.00,
          "brackets": [
            { "threshold": 0.00, "rate": 0.1050, "constant": 0.00 },
            { "threshold": 54532.00, "rate": 0.1250, "constant": 1091.00 },
            { "threshold": 155805.00, "rate": 0.1450, "constant": 4207.00 }
          ]
        },
        "YT": {
          "basicPersonalAmount": 16452.00,
          "territorialCanadaEmploymentAmount": true,
          "brackets": [
            { "threshold": 0.00, "rate": 0.0640, "constant": 0.00 },
            { "threshold": 58523.00, "rate": 0.0900, "constant": 1522.00 },
            { "threshold": 117045.00, "rate": 0.1090, "constant": 3745.00 },
            { "threshold": 181440.00, "rate": 0.1280, "constant": 7193.00 },
            { "threshold": 500000.00, "rate": 0.1500, "constant": 18193.00 }
          ]
        }
      }
    }
  ]
}
""";
}
