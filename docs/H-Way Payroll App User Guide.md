# H-Way Payroll App User Guide

This guide explains how to use the H-Way Payroll App. It is written for regular app users, not developers.

Use this guide when you need to add employers, add employees, run payroll, calculate deductions, save pay records, print or send paystubs, review payroll history, save PD7 records, view T4 totals, or update payroll settings.

## Basic Navigation

The app uses tabs/pages across the top or side of the window.

Main pages:

- Payroll Workspace: run the current payroll for a company.
- Payroll History: review payroll that has already been saved.
- Payroll Edit: correct an existing payroll record.
- Employees: add, search, edit, or remove employees.
- Employers: add, search, edit, or remove employers.
- PD7 Records: review and save monthly remittance totals.
- T4 Records: review company and employee T4 box totals.
- Settings: manage pay periods, yearly limits, calculation values, and year reset.

Most pages work the same way:

1. Search for a company or employee.
2. Select a result from the list.
3. Enter or review the information.
4. Click the save, calculate, print, or delete button for that page.
5. Read the message shown on the page after the action finishes.

## Important Words Used In The App

RP Number: The employer payroll account number used to connect employers, employees, payroll records, PD7 records, and T4 records.

Employee Number: The employee's internal number.

Employer Group: The pay schedule for the employer.

- A - Monthly
- B - Semi-Monthly
- C - Bi-weekly A
- D - Bi-weekly B

Period #: The current pay period number for the employer. The app advances this after payroll is saved from the Payroll Workspace.

Salary: A fixed amount paid for the pay period.

Rate: The hourly rate. Use this with Regular Hours, OT Hours, Public H., or Stat H.

Gross: The total earnings before deductions.

Vacation: The app calculates vacation at 4% of gross.

Vacation Setting:

- Accumulate: Vacation is earned and kept in the employee's vacation balance.
- Paid Out: Vacation is paid on the current paystub.

Tax: Income tax deducted for the pay period.

CPP1 and CPP2: Canada Pension Plan deductions.

EI: Employment Insurance deduction.

Other Deductions: Any extra deduction that is not tax, CPP, or EI.

Net Pay: The amount paid to the employee after deductions.

PD7: Employer remittance summary.

T4: Year-end employee tax slip totals.

## Recommended Payroll Workflow

Use this order when setting up and running payroll.

1. Go to Employers and add or confirm the employer.
2. Go to Employees and add or confirm the employees for that employer.
3. Go to Settings and confirm the pay schedule and payroll calculation values are correct, if you are responsible for settings.
4. Go to Payroll Workspace.
5. Search for the employer.
6. Enter salary or hourly pay information for each employee.
7. Click Calculate Payroll Deductions for each employee.
8. Review gross, vacation, tax, CPP, EI, other deductions, and paystub preview.
9. Print or send paystubs if needed.
10. Click Save All Employees.
11. Review Payroll History and PD7 Records if needed.

## Payroll Workspace Page

Use Payroll Workspace to run payroll for the current employer period.

### Select A Company

1. Open Payroll Workspace.
2. In Company, type the RP number or employer name.
3. Select the employer from the dropdown list.
4. The Selected Employer section will show the RP number, employer name, address, city, postal code, employer group, and current period number.
5. The app loads all employees connected to that employer.

If no employees are found, the page shows a message saying no employees were found for the employer.

### Enter Pay For A Salaried Employee

Use Salary when the employee is paid a fixed amount for the pay period.

1. Find the employee's card under Employee Payroll.
2. In Salary, enter the fixed pay amount for this period.
3. Leave Rate and the hour fields as 0 unless the employee also needs hourly pay added.
4. Choose the Pay Date.
5. Choose the Vacation Setting:
   - Accumulate if vacation should be saved in the balance.
   - Paid Out if vacation should be paid on this paystub.
6. Click Calculate Payroll Deductions.
7. Review Gross, Vacation, Tax, CPP1, CPP2, EI, and Net Pay on the paystub preview if needed.

Important: If you enter a Salary amount, the app clears the Rate field. This prevents accidentally paying the same employee as both salary and hourly.

### Enter Pay For An Hourly Employee

Use Rate and hours when the employee is paid by the hour.

1. Find the employee's card under Employee Payroll.
2. In Rate, enter the hourly rate.
3. In Regular Hours, enter regular hours worked.
4. In OT Hours, enter overtime hours.
5. In Public H., enter public holiday hours, if any.
6. In Stat H., enter statutory holiday hours, if any.
7. Leave Salary as 0.
8. Choose the Pay Date.
9. Choose the Vacation Setting.
10. Click Calculate Payroll Deductions.
11. Review the calculated values.

Important: If you enter a Rate amount, the app clears the Salary field.

### How The App Calculates Gross Pay

When you click Calculate Payroll Deductions, the app calculates gross pay like this:

- Salary is added directly.
- Regular pay is Rate x Regular Hours.
- Overtime pay is Rate x 1.5 x OT Hours.
- Public holiday pay is Rate x Public H.
- Stat holiday pay is Rate x 1.5 x Stat H.
- Gross is the total of those amounts.
- Vacation is calculated as 4% of Gross.

### How To Calculate Tax, CPP, And EI

1. Enter the employee's salary or hourly pay information.
2. Confirm the Pay Date.
3. Confirm the Vacation Setting.
4. Click Calculate Payroll Deductions.
5. The app fills in Tax, CPP1, CPP2, and EI.
6. Review the numbers before saving.

The app calculates deductions from the saved payroll calculation settings and the employee's province, birth date, EI exempt setting, year-to-date totals, and current payroll year limits.

If CPP or EI cannot be edited, it usually means the employee is not eligible or has reached the saved yearly maximum.

### Other Deductions

Use Other Deductions for extra deductions that are not tax, CPP, or EI.

1. Calculate payroll deductions first.
2. Enter the extra deduction amount in Other Deductions.
3. Preview the paystub to confirm the net pay looks correct.

### Vacation Balance

The Vacation Balance section shows:

- Vacation Used: the amount of vacation being used or paid this period.
- Vacation YTD: vacation earned so far this year.
- Balance: remaining vacation balance.
- Used YTD: vacation already used this year.

If Vacation Setting is Paid Out, the app treats the current vacation amount as used/paid.

If Vacation Setting is Accumulate, you can enter Vacation Used manually.

### Preview A Paystub

1. Enter the pay information.
2. Click Calculate Payroll Deductions.
3. Click Preview Paystub.
4. A paystub preview window opens.
5. Review the employee name, pay period, earnings, deductions, vacation balance, and net pay.
6. Click Close when finished.

### Print A Paystub

1. Enter and calculate the employee's payroll.
2. Click Print Paystub.
3. The app creates a PDF paystub and opens it.
4. Print or save the PDF from the PDF viewer.

The PDF file name uses the employee name and today's date.

### Send All Pay Stubs

Use Send All Pay Stubs to email paystubs to employees.

1. Enter and calculate payroll for all employees.
2. Make sure each employee has a valid email address.
3. Click Send All Pay Stubs.
4. If some employees do not have valid emails, the app warns you and lets you continue with the available emails or cancel.
5. The app creates each paystub PDF and emails it.
6. A summary shows how many were sent, skipped, or failed.

If email settings are incomplete, the app shows an email settings warning and does not send.

### Save All Employees

Click Save All Employees only after reviewing the payroll.

1. Make sure each employee has the correct pay information.
2. Click Calculate Payroll Deductions for each employee that is being paid.
3. Review paystubs if needed.
4. Click Save All Employees.
5. The app saves the payroll records for the current period.
6. The employer's next period number is updated.
7. The app also auto-saves the PD7 amount for the matching month and year when possible.
8. A message shows how many employees were saved and what the next period is.

## Payroll History Page

Use Payroll History to review payroll that was already saved.

### Load Payroll History

1. Open Payroll History.
2. Search by RP number or employer name.
3. Select the employer from the dropdown list.
4. Choose filters:
   - Term
   - Year
   - Month
   - Period
   - Employee name or employee number, if you only want one employee
5. Click Load History.

The page shows:

- Number of employees.
- Number of payroll records.
- Gross total.
- Total pay.
- Total deductions.
- Net pay.
- A detailed card for each saved employee payroll record.

### Review A History Record

Each record shows:

- Employee name and employee number.
- Period number.
- Term/group.
- Period beginning and ending dates.
- Pay date.
- Date saved.
- Salary, rate, hours, gross, vacation, and total pay.
- Tax, CPP1, CPP2, EI, other deductions, total deductions, and net pay.
- Year-to-date totals.
- Vacation balance.
- Total PD7.

### Clear History Filters

Click Clear to remove the selected company and filters and start again.

## Payroll Edit Page

Use Payroll Edit when a saved payroll record needs to be corrected.

This page is for fixing an existing record, not for running a new payroll.

### Select The Payroll Record To Edit

1. Open Payroll Edit.
2. In Company, type the RP number or employer name.
3. Click Search Companies or press Enter.
4. Select the company from the list.
5. Open the Employee dropdown.
6. Select the employee payroll record.
7. The selected employee's payroll information appears.

### Edit Earnings

You can edit:

- Salary
- Rate
- Regular Hours
- Overtime Hours
- Public H.
- Stat H.
- Vacation Setting

Gross and Vacation are calculated by the app.

Important: If you change salary, rate, or hours, click Recalculate Payroll Deductions before saving. The app requires this.

### Edit Deductions

You can edit:

- Tax
- CPP1
- CPP2
- EI
- Other Deduction

Net Pay updates from the earnings and deductions.

CPP and EI fields may be disabled if the employee cannot contribute or has reached the saved yearly maximum.

### Recalculate Payroll Deductions

1. Make your correction.
2. Click Recalculate Payroll Deductions.
3. The app recalculates Gross, Vacation, Tax, CPP1, CPP2, EI, and Net Pay.
4. Review the results.
5. Click Save Changes.

If you try to save after changing earnings without recalculating, the app shows a message telling you to recalculate first.

### Preview Or Print A Corrected Paystub

1. Select the company and employee.
2. Make edits.
3. Click Recalculate Payroll Deductions if earnings changed.
4. Click Preview Paystub or Print Paystub.
5. Review or print the corrected PDF.

### Save The Correction

1. Make sure the record is correct.
2. Click Save Changes.
3. The app saves the corrected payroll record.
4. If the latest payroll history snapshot exists, it is updated.
5. If a saved PD7 record exists for that period, the app recalculates it.
6. Read the success message to see what was updated.

## Employees Page

Use Employees to add, search, update, or delete employee records.

### Search Employees

1. Open Employees.
2. Optional: select an employer from the Employer Filter list.
3. Or type into Employee Search.
4. You can search by RP number, employee number, first name, or last name.
5. Click Search.
6. Matching employees appear in the list.

### Add A New Employee

1. Open Employees.
2. Click + Add New Employee.
3. Fill in the employee information:
   - RP Number
   - Employee Number
   - First Name
   - Last Name
   - SIN
   - Address
   - City
   - Province
   - Postal Code
   - Email
4. In Payroll Eligibility, check Hire Date if you want to save a hire date, then choose the date.
5. Check Birth Date if you want to save a birth date, then choose the date.
6. Check EI Exempt if the employee should not have EI deducted.
7. Click Save.

Required fields:

- RP Number
- Employee Number
- First Name
- Last Name

The hire date and birth date cannot be in the future.

The employee's province matters because the payroll deduction calculation uses the employee province.

### Edit An Employee

1. Search for the employee.
2. Click the employee row.
3. The Employee Details form opens.
4. Edit the information.
5. Click Save.
6. The app shows Employee updated successfully.

When editing an existing employee, RP Number and Employee Number are read-only on the combined Employees page.

### Delete An Employee

1. Search for the employee.
2. Click the employee row.
3. Click Delete.
4. Confirm the delete message.
5. The app removes the employee and shows Employee deleted successfully.

Only delete an employee if you are sure. Deleted employee records cannot be restored from inside the app.

### Cancel Editing

Click Cancel in Employee Details to close the form and return to the previous search results.

## Employers Page

Use Employers to add, search, update, or delete employer records.

### Search Employers

1. Open Employers.
2. Type the RP number or employer name in Search.
3. Click Search.
4. Matching employers appear in the list.
5. Click an employer row to view or edit its details.

### Add A New Employer

1. Open Employers.
2. Click + Add New Employer.
3. Fill in:
   - RP Number
   - Employer Name
   - Address
   - City
   - Postal Code
   - Employer Group
   - Fee
4. Click Save.

Required fields:

- RP Number
- Employer Name

For a new employer, the app starts the Period # at 1.

### Choose The Employer Group

The employer group controls the pay schedule.

- A - Monthly
- B - Semi-Monthly
- C - Bi-weekly A
- D - Bi-weekly B

Choose this carefully before running payroll. The group affects pay periods, payroll history, PD7, and T4 calculations.

### Edit An Employer

1. Search for the employer.
2. Click the employer row.
3. Edit the fields.
4. Click Save.
5. The app shows Employer updated successfully.

When editing an existing employer, the RP Number is read-only on the combined Employers page.

The Period # field is editable only when editing an existing employer. Use it carefully because it controls the next payroll period.

### Delete An Employer

1. Search for the employer.
2. Click the employer row.
3. Click Delete.
4. Confirm the delete message.
5. The app removes the employer and shows Employer deleted successfully.

Only delete an employer if you are sure. Deleting an employer can affect connected payroll work.

### Cancel Editing

Click Cancel in Employer Details to close the form and return to the previous search results.

## PD7 Records Page

Use PD7 Records to review and save monthly remittance totals for an employer.

### Select A Company

1. Open PD7 Records.
2. Search by RP number or employer name.
3. Select the employer from the dropdown list.
4. The app shows the selected employer, fee, and PD7 total for the selected year.

### Select The Month And Year

1. Choose the Month.
2. Enter the Year.
3. The app calculates the matching period summary and remittance totals.

The app calculates:

- Gross
- Net Total
- Employer Contribution
- Active Employees
- PD7 Total
- Fee
- Transfer

### Calculation Check

The Calculation Check compares:

Net Total + PD7 Total

with:

Gross + Employer Contribution

If the values match, the check is green and says the amounts match.

If they do not match, the app shows the difference.

### Enter Received Amount

1. Enter the amount received in Received.
2. If there was a saved amount for the same month and year, it appears as Paid So Far.
3. The app compares Paid So Far + Received to Transfer.
4. Transfer Check tells you whether the received total matches the transfer amount.

### Save PD7

1. Select the company.
2. Select month and year.
3. Review the calculated totals.
4. Enter Received.
5. Confirm the Transfer Check.
6. Click Save PD7.
7. The app saves the PD7 record and shows PD7 saved successfully.

The app does not allow negative values for PD7 amounts.

### Clear PD7 Page

Click Clear to remove the selected company and reset the page to the current month and year.

## T4 Records Page

Use T4 Records to review company T4 totals and employee T4 box amounts.

### View T4 Totals

1. Open T4 Records.
2. Enter the tax year.
3. Search by RP number or employer name.
4. Select the employer from the dropdown list.
5. The app calculates T4 totals for that company and year.

The company totals include:

- Box 14 - Employment Income
- Box 24 - EI Insurable Earnings
- Box 26 - CPP/QPP Pensionable Earnings
- Box 22 - Income Tax Deducted
- Box 16 - CPP1 Contributions
- Box 16A - CPP2 Contributions
- Box 18 - EI Premiums

The page also shows the tax year maximums used for EI and CPP.

### Review Employee T4 Slips

Below the company totals, each employee has a T4 summary card.

Each employee card shows:

- Employee name
- Employee number
- Address
- SIN
- Box 14
- Box 24
- Box 26
- Box 22
- Box 16
- Box 16A
- Box 18

### Clear T4 Page

Click Clear to remove the selected company and reset the tax year to the current year.

## Settings Page

Settings should usually be changed only by the person responsible for payroll setup.

Changes on this page affect payroll calculations and future payroll work.

### Year Reset

Use Reset Year at year end when you are ready to start payroll totals again.

1. Enter the payroll year.
2. Click Reset Year.
3. Read the confirmation message.
4. Click Reset Year only if you are sure.

The reset does this:

- Clears current payroll amounts.
- Clears year-to-date payroll totals.
- Resets employer periods to 1.

The reset keeps:

- Employers
- Employees
- Pay schedules
- Deduction limits
- Calculation values
- Payroll history
- PD7 records

### Bi-weekly Settings

Use Bi-weekly Settings to set the pay period schedules for Bi-weekly A and Bi-weekly B employers.

For each schedule:

1. Choose the Last period ending date.
2. Review Period 1 beginning.
3. Review the generated period list.
4. Review any three-period months.
5. Click Save Bi-weekly A or Save Bi-weekly B.

To save both schedules at once, click Save Both.

To reload saved schedules, click Reload Saved.

### Yearly CPP/EI Limits

Use Yearly CPP/EI Limits to review or update the maximum deductions for a year.

1. Select a saved year from Saved Years.
2. Review the Year, Maximum CPP1, Maximum CPP2, and Maximum EI fields.
3. Edit the values if needed.
4. Click Save Limits.

These limits affect whether CPP and EI can still be deducted for employees who are close to yearly maximums.

### Payroll Calculations

Use Payroll Calculations to review or update the saved calculation values used by the app.

1. Select CPP, EI, or Income Tax from Stored Calculations.
2. Review the effective date and fields shown.
3. Update the required values.
4. For Income Tax, review federal amounts, federal brackets, province settings, province brackets, and Ontario health premium bands if shown.
5. Click Save Yearly Values.

Be careful on this page. Incorrect values can cause incorrect payroll deductions.

## Simple Examples

### Example: Run Payroll For A Salaried Employee

1. Open Payroll Workspace.
2. Search for and select the employer.
3. Find the employee.
4. Enter 1500 in Salary.
5. Leave Rate and hours as 0.
6. Choose the Pay Date.
7. Choose Accumulate or Paid Out for Vacation Setting.
8. Click Calculate Payroll Deductions.
9. Review Tax, CPP1, CPP2, EI, and Net Pay.
10. Click Preview Paystub if you want to check the paystub.
11. Click Save All Employees when all employee rows are ready.

### Example: Run Payroll For An Hourly Employee

1. Open Payroll Workspace.
2. Search for and select the employer.
3. Find the employee.
4. Enter 20 in Rate.
5. Enter 40 in Regular Hours.
6. Enter any overtime, public holiday, or stat holiday hours.
7. Leave Salary as 0.
8. Click Calculate Payroll Deductions.
9. Review the result.
10. Save when ready.

### Example: Correct A Payroll Record

1. Open Payroll Edit.
2. Search for the company.
3. Select the company.
4. Select the employee.
5. Change the incorrect salary, rate, hours, or deduction.
6. Click Recalculate Payroll Deductions if earnings changed.
7. Preview or print the corrected paystub if needed.
8. Click Save Changes.

### Example: Add A New Employer

1. Open Employers.
2. Click + Add New Employer.
3. Enter RP Number and Employer Name.
4. Add address, city, postal code, employer group, and fee.
5. Click Save.

### Example: Add A New Employee

1. Open Employees.
2. Click + Add New Employee.
3. Enter RP Number, Employee Number, First Name, and Last Name.
4. Enter SIN, address, city, province, postal code, and email if available.
5. Add hire date, birth date, and EI exempt setting if needed.
6. Click Save.

## Common Problems And What To Do

No company appears when searching:

- Check the spelling.
- Try searching by RP number.
- Make sure the employer exists on the Employers page.

No employee appears for a company:

- Make sure the employee exists on the Employees page.
- Make sure the employee has the correct RP Number.

CPP or EI cannot be edited:

- The employee may not be eligible.
- The employee may be EI exempt.
- The employee may have reached the saved yearly maximum.
- Check the employee birth date, EI exempt setting, and yearly CPP/EI limits.

Tax, CPP, or EI looks wrong:

- Make sure the employee province is correct.
- Make sure the pay date and payroll year are correct.
- Make sure payroll calculation settings are correct.
- Click Calculate Payroll Deductions again after changing earnings.

The app says to recalculate before saving:

- Click Recalculate Payroll Deductions.
- Review the new totals.
- Click Save Changes again.

Paystub email did not send:

- Make sure the employee has a valid email address.
- Make sure email settings are configured.
- Try printing the paystub PDF instead.

PD7 transfer check does not match:

- Review the calculated Transfer amount.
- Check Paid So Far.
- Check the new Received amount.
- The app compares Paid So Far + Received with Transfer.

## Final Review Before Saving Payroll

Before clicking Save All Employees, check:

- Correct employer is selected.
- Correct period is shown.
- Each paid employee has salary or hourly information entered.
- Calculate Payroll Deductions was clicked for each paid employee.
- Tax, CPP, EI, and Other Deductions look correct.
- Vacation Setting is correct.
- Pay Date is correct.
- Paystub preview looks correct if you reviewed it.

After saving:

- The period advances.
- Payroll history is created.
- PD7 is auto-saved when the app can match the period to a month and year.
