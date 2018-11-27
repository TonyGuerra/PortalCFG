//--------------- PortalCFG ---------------
//--------- Antonio C Ferreira ------------

function Init () {
  var all = document.getElementsByTagName("*");

  for (var i=0, max=all.length; i < max; i++) {
     // Do something with the element here
 if ((all[i].title) && (all[i].title == 'data')) {
        all[i].addEventListener( 'keypress', fdata, false );
        all[i].addEventListener( 'change', vdata, false );
     }
  }

}

if (document.all || document.getElementById){sender="event.srcElement"}else{sender="e.target"}

function fdata(event){
    if (!event) var event = window.event;
alvo=eval(sender);
if (document.all || document.getElementById){
x=event.keyCode;
}else{
x=event.which
}
if (x==8){return true;}
if(document.all || document.getElementById){
if (x>47 && x<58){
x=x-48;
valor=alvo.value;
if (valor.length==0){
if (x>3){alvo.value="0"}
}
if (valor.length==2){
if (x>1){alvo.value+="/0"}else{alvo.value+="/"}
}
if (valor.length==5){
if (x>3){alvo.value+="/19"}else{
if (x>0){alvo.value+="/"}else{alvo.value+="/20"}
}
}
return true;
}
}else{
if (x>46 && x<58){return true;}
}
event.preventDefault();
return false;
}

function vdata(event){

if (!event) var event = window.event;

alvo=eval(sender);
var parts = alvo.value.split('/');

var dData1 = new Date(parts[2], parts[1]-1, parts[0]);
var dData2 = new Date(parts[2], parts[1]-1, 1);

if  ((parts[1] > 12) || (dData1.getMonth() != dData2.getMonth())) {
    alert('data invalida!');
alvo.value = '';
}

return true;

}


function jsGatilho(cGatilho, cCodigo) { 

    var result = 0;
        
    http_request = false;

    if  (window.XMLHttpRequest) { // Mozilla, Safari,...
        http_request = new XMLHttpRequest();
        if  (http_request.overrideMimeType) {
            http_request.overrideMimeType('text/xml');
        }
    }  else if (window.ActiveXObject) { // IE
       try {
           http_request = new ActiveXObject("Msxml2.XMLHTTP");
       }   catch (e) {
           try {
               http_request = new ActiveXObject("Microsoft.XMLHTTP");
           } catch (e) {}
       }
    }

    if  (!http_request) {
        alert('Desistindo :( Nao consigo criar o XMLHTTP instance');
        return false;
    } 

    //alert(cCodigo);                          
                                         
    url = "gatilho?cLogin="+document.getElementById("cLogin").value+"&cSessao="+document.getElementById("cSessao").value+"&cGatilho="+cGatilho+"&cCodigo="+cCodigo;
        
    http_request.onreadystatechange = function() { jsRetornoGatilho(); }
    http_request.open('GET', url, true);
    http_request.send(null);

} 

function jsRetornoGatilho()
{

    if  (http_request.readyState == 4) {
    //alert(http_request.status);
        if  (http_request.status == 200) {  
            
             result = http_request.responseText;

             result = result.replace('&nbsp;','');

              if  ((result == 0) || (result.length <= 0)) {

                   alert("Gatilho n&atildeo executado!");   

               }   else {   
        
                    if  (result == 'ERRO') {
                         alert("Problema para executar o gatilho!");
                    } else {
                         var aVal = result.split("|");

                         for (var i=0, max=aVal.length; i < max; i+=2) {
                               document.getElementById(aVal[i]).value = aVal[i+1];
                         }
                    }
             }          
        }   else {
            alert('Nao consegui executar o gatilho!.');
        }//else
    }//if

}//function


function fXYAcento(xTexto) {

   var cTexto = xTexto.replace('À','\xC0');

   cTexto = cTexto.replace('Á','\xC1');
   cTexto = cTexto.replace('Â','\xC2');
   cTexto = cTexto.replace('Ã','\xC3');
   cTexto = cTexto.replace('Ä','\xC4');
   cTexto = cTexto.replace('Å','\xC5');
   cTexto = cTexto.replace('Æ','\xC6');
   cTexto = cTexto.replace('Ç','\xC7');
   cTexto = cTexto.replace('È','\xC8');
   cTexto = cTexto.replace('É','\xC9');
   cTexto = cTexto.replace('Ê','\xCA');
   cTexto = cTexto.replace('Ë','\xCB');
   cTexto = cTexto.replace('Ì','\xCC');
   cTexto = cTexto.replace('Í','\xCD');
   cTexto = cTexto.replace('Î','\xCE');
   cTexto = cTexto.replace('Ï','\xCF');
   cTexto = cTexto.replace('Ð','\xD0');
   cTexto = cTexto.replace('Ñ','\xD1');
   cTexto = cTexto.replace('Ò','\xD2');
   cTexto = cTexto.replace('Ó','\xD3');
   cTexto = cTexto.replace('Ô','\xD4');
   cTexto = cTexto.replace('Õ','\xD5');
   cTexto = cTexto.replace('Ö','\xD6');
   cTexto = cTexto.replace('Ø','\xD8');
   cTexto = cTexto.replace('Ù','\xD9');
   cTexto = cTexto.replace('Ú','\xDA');
   cTexto = cTexto.replace('Û','\xDB');
   cTexto = cTexto.replace('Ü','\xDC');
   cTexto = cTexto.replace('Ý','\xDD');
   cTexto = cTexto.replace('Þ','\xDE');
   cTexto = cTexto.replace('ß','\xDF');
   cTexto = cTexto.replace('à','\xE0');
   cTexto = cTexto.replace('á','\xE1');
   cTexto = cTexto.replace('â','\xE2');
   cTexto = cTexto.replace('ã','\xE3');
   cTexto = cTexto.replace('ä','\xE4');
   cTexto = cTexto.replace('å','\xE5');
   cTexto = cTexto.replace('æ','\xE6');
   cTexto = cTexto.replace('ç','\xE7');
   cTexto = cTexto.replace('è','\xE8');
   cTexto = cTexto.replace('é','\xE9');
   cTexto = cTexto.replace('ê','\xEA');
   cTexto = cTexto.replace('ë','\xEB');
   cTexto = cTexto.replace('ì','\xEC');
   cTexto = cTexto.replace('í','\xED');
   cTexto = cTexto.replace('î','\xEE');
   cTexto = cTexto.replace('ï','\xEF');
   cTexto = cTexto.replace('ð','\xF0');
   cTexto = cTexto.replace('ñ','\xF1');
   cTexto = cTexto.replace('ò','\xF2');
   cTexto = cTexto.replace('ó','\xF3');
   cTexto = cTexto.replace('ô','\xF4');
   cTexto = cTexto.replace('õ','\xF5');
   cTexto = cTexto.replace('ö','\xF6');
   cTexto = cTexto.replace('ø','\xF8');
   cTexto = cTexto.replace('ù','\xF9');
   cTexto = cTexto.replace('ú','\xFA');
   cTexto = cTexto.replace('û','\xFB');
   cTexto = cTexto.replace('ü','\xFC');
   cTexto = cTexto.replace('ý','\xFD');
   cTexto = cTexto.replace('þ','\xFE');

   return cTexto;

}//fXYAcento()

