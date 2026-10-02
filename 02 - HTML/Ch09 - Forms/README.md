# Chapter 9: A labelled HTML form

Open `demo-accessible-form.html` from the course index after running `npm start` at the repository root. Read the HTML first; then inspect the relevant CSS or JavaScript. The styling around the example is only for readability.

## Instructor demonstration (about 12–15 minutes)

1. Activate labels and use Tab through the controls.
2. Submit a blank name and invalid email to observe native validation.
3. Submit valid values and inspect the POST body in Network.

## Student exercise (about 20–22 minutes)

Build a form using labels, name attributes, native constraints and a grouped choice. Submit it to /form-echo. The classroom echo endpoint does not save the data.

## Show briefly

Custom JavaScript validation belongs after the JavaScript lessons. The local echo endpoint requires npm start; static hosting alone will not process the form.

After the small demo, open `studentlab.html` for the fuller slide-16 registration exercise: personal/academic fieldsets, email, tel, date, bounded number, select, radio, textarea and a described pattern field. Remove one name attribute and compare the submitted payload.
