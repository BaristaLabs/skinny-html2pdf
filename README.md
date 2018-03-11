Skinny Html2PDF
======

Skinny Html2PDF is a .Net Core 2.1 based microservice that converts Html to PDF using [chrome-dev-tools-runtime](https://github.com/BaristaLabs/chrome-dev-tools-runtime)

This project also contains a dockerfile that will build and run the microservice in a minimal environment.

While perhaps interesting in itself, some effort was made to make this stack 'skinny':

 - Built using .Net Core 2.1 (Preview) (2.1 is skinner than 2.0)
 - Docker image based on Alpine Linux (Alpine is skinner than standard debian)
 - TODO: Tree Trimming tools (seems to be broken in current Alpine)

Image size without Chrome: 160mb. With Chrome: 537mb.
(Of course, Chrome is required, this metric is just to illustrate the base vs base + chrome image size)

Available on docker hub here: https://hub.docker.com/r/oceanswave/skinny-html2pdf/

### Getting Started
---
[Docker](https://www.docker.com/get-docker) must be installed and running.

``` bash
docker pull oceanswave/skinny-html2pdf
docker run -d -p 80:8080 --shm-size=1gb --cap-add SYS_ADMIN oceanswave/skinny-html2pdf
```

Once the container is running, the services will be hosted at localhost:80 and exposes the following endpoints:

http://localhost/api/html2pdf?url=https://medium.com/netflix-techblog/embracing-the-differences-inside-the-netflix-api-redesign-15fd8b3dc49d&fileName=myarticle.pdf

Parameters:
 - uri: full uri of the web page to render
 - width: Width in pixels of the page to render
 - height: Height in pixels of the page to render
 - filename: content-disposition filename value
 
http://localhost/api/html2image?url=https://www.pexels.com/photo/white-and-yellow-flower-with-green-stems-36764/

Parameters:
 - uri: full uri of the web page to render
 - width: Width in pixels of the page to render
 - height: Height in pixels of the page to render
 - filename: content-disposition filename value

The service also supports ad-hoc conversion of Html to PDF.

Simply POST to http://localhost/api/html2pdf with the body containing of the HTML to convert into PDF.

Parameters:
 - width: Width in pixels of the page to render
 - height: Height in pixels of the page to render
 - filename: content-disposition filename value

### Hosting on Azure
---

Generally, follow the instructions on https://docs.microsoft.com/en-us/azure/app-service/containers/tutorial-custom-docker-image

``` Powershell
az group create --name skinnyHtml2PdfResourceGroup --location "East US 2"
az appservice plan create --name skinnyHtml2PdfServicePlan --resource-group skinnyHtml2PdfResourceGroup --sku B1 --is-linux
az webapp create --resource-group skinnyHtml2PdfResourceGroup --plan skinnyHtml2PdfServicePlan --name skinny-html2pdf --deployment-container-image-name oceanswave/skinny-html2pdf:latest
```

> Note: Due to https://bugs.chromium.org/p/chromium/issues/detail?id=736452#c33 and the inability to pass parameters to docker run with Azure Web Apps for Containers,
> responses are sporatic and may return 0-byte responses due to the chrome session running out of memory. Use a VM host with docker compose or wait until Alpine has
> support for Chrome 65+

### Development
---

As .Net Core 2.1 is still in development, the .Net Core 2.1 Preview SDK must be downloaded and installed. 

The latest version of Chromium available for Alpine at the time of this writing is Chrome 64.

``` bash
git clone https://github.com/baristalabs/skinny-html2pdf
cd skinny-html2pdf
docker-compose build
docker-compose up -d
```

The microservice can be built without docker-compose as well.

``` bash
git clone https://github.com/baristalabs/skinny-html2pdf
cd skinny-html2pdf
docker build -rm -t oceanswave/skinnyhtml2pdf:latest -f BaristaLabs.SkinnyHtml2Pdf.Web/Dockerfile .
docker run -d -p 80:8080 --shm-size=1gb --cap-add SYS_ADMIN oceanswave/skinny-html2pdf
```
