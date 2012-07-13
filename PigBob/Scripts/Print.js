//Generating Pop-up Print Preview page
function getPrint(print_area, datestring)
{
	//Creating new page
	var pp = window.open();
	//Adding HTML opening tag with <HEAD> … </HEAD> portion
	pp.document.writeln('<html><head><title>Print Preview</title><link href="/Content/PrintStyle.css" type="text/css" rel="stylesheet"></head>')
	//Adding Body Tag
	pp.document.writeln('<body bottomMargin="0" leftMargin="0" topMargin="0" rightMargin="0">');
	//Adding form Tag
	pp.document.writeln('<form  method="post">');
    //Writing the title
	pp.document.writeln('<h1 align="center">Pigbob Order - ' + datestring + '</h1>');
	//Writing print area of the calling page
	pp.document.writeln('<p align="center">');
	pp.document.writeln(document.getElementById(print_area).innerHTML);
	//Creating two buttons Print and Close
	pp.document.writeln('</p><p align="center">');
	pp.document.writeln('<input id="PRINT" type="button" value="Print" onclick="javascript:location.reload(true);window.print();">');
	pp.document.writeln('<input id="CLOSE" type="button" value="Close" onclick="window.close();">');
	pp.document.writeln('</p>');
	//Ending Tag of </form>, </body> and </HTML>
	pp.document.writeln('</form></body></html>');
}