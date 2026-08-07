/**
 * FYTALA Mermaid Theme Configuration
 *
 * How to use:
 *   1. Include this script BEFORE initializing Mermaid:
 *      <script src="./mermaid-theme-config.js"></script>
 *   2. Then initialize Mermaid:
 *      mermaid.initialize({ startOnLoad: true, theme: 'base', themeVariables: fytalaMermaidTheme });
 *
 * Or inline:
 *   <script>
 *     mermaid.initialize({
 *       startOnLoad: true,
 *       theme: 'base',
 *       themeVariables: {
 *         primaryColor: '#F0FFF0',
 *         primaryTextColor: '#2F4F4F',
 *         primaryBorderColor: '#518D8F',
 *         lineColor: '#5178A8',
 *         secondaryColor: '#E8F4EE',
 *         tertiaryColor: '#FDF2CF',
 *         fontFamily: 'Poppins, Segoe UI, sans-serif'
 *       }
 *     });
 *   </script>
 */

const fytalaMermaidTheme = {
  /* Node styling */
  primaryColor: '#F0FFF0',
  primaryTextColor: '#2F4F4F',
  primaryBorderColor: '#518D8F',

  /* Lines / edges */
  lineColor: '#5178A8',

  /* Secondary surfaces (clusters, groups) */
  secondaryColor: '#E8F4EE',
  tertiaryColor: '#FDF2CF',

  /* Background */
  background: '#FFFFFF',

  /* Text */
  mainBkg: '#F0FFF0',
  secondBkg: '#E8F4EE',
  titleColor: '#556B2F',

  /* Sequence diagram */
  actorBorder: '#518D8F',
  actorBkg: '#F0FFF0',
  actorTextColor: '#2F4F4F',
  actorLineColor: '#518D8F',
  signalColor: '#5178A8',
  signalTextColor: '#2F4F4F',

  /* Loops / activation */
  loopTextColor: '#2F4F4F',
  activationBorderColor: '#518D8F',
  activationBkgColor: '#FDF2CF',

  /* Sections (Gantt) */
  sectionBkgColor: '#E8F4EE',
  altSectionBkgColor: '#F0FFF0',
  gridColor: 'rgba(0,0,0,0.08)',

  /* Task (Gantt) */
  taskBkgColor: '#518D8F',
  taskTextColor: '#2F4F4F',
  taskTextLightColor: '#F0FFF0',
  taskTextOutsideColor: '#2F4F4F',
  activeTaskBkgColor: '#98D481',
  activeTaskBorderColor: '#556B2F',
  gridColor: 'rgba(0,0,0,0.06)',

  /* Today line (Gantt) */
  todayLineColor: '#518D8F',

  /* Crit path (Gantt) */
  critBkgColor: '#FDF2CF',
  critBorderColor: '#556B2F',
  doneTaskBkgColor: '#E8F4EE',
  doneTaskBorderColor: '#556B2F',

  /* Git graph */
  git0: '#293F5F',
  git1: '#50639C',
  git2: '#5178A8',
  git3: '#518D8F',
  git4: '#98D481',
  git5: '#FDF2CF',
  git6: '#556B2F',
  git7: '#2F4F4F',

  /* Pie / Mindmap */
  pie1: '#293F5F',
  pie2: '#50639C',
  pie3: '#5178A8',
  pie4: '#518D8F',
  pie5: '#98D481',
  pie6: '#FDF2CF',
  pie7: '#556B2F',
  pie8: '#2F4F4F',
  pieTitleTextSize: '18px',
  pieTitleTextColor: '#556B2F',
  pieSectionTextSize: '14px',
  pieSectionTextColor: '#2F4F4F',

  /* Font */
  fontFamily: 'Poppins, Segoe UI, system-ui, sans-serif',
  fontSize: '14px'
};

/* Auto-initialize if mermaid is present */
if (typeof mermaid !== 'undefined') {
  mermaid.initialize({
    startOnLoad: true,
    theme: 'base',
    themeVariables: fytalaMermaidTheme
  });
}
