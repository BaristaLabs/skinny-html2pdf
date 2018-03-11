Skinny Html2PDF
======

Skinny Html2PDF is a .Net Core 2.1 based microservice that converts Html to PDF using [chrome-dev-tools-runtime](https://github.com/BaristaLabs/chrome-dev-tools-runtime)

This project also contains a dockerfile that will build and run the microservice in a minimal environment.

 - Built using .Net Core 2.1 
 - Docker image based on Alpine Linux
 - Image size without Chrome: 160mb. With Chrome: 537mb. (Of course Chrome is required.)

##### Getting Started
---
[Docker](https://www.docker.com/get-docker) must be installed and running.

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
docker run -d -p 80:8080 --cap-add SYS_ADMIN oceanswave/skinnyhtml2pdf
```

Once the container is running, the services will be hosted at localhost:80.

The services can now be used, for instance:

http://localhost/api/html2pdf?url=https://medium.com/netflix-techblog/embracing-the-differences-inside-the-netflix-api-redesign-15fd8b3dc49d&fileName=myarticle.pdf

http://localhost/api/html2image?url=https://www.pexels.com/photo/white-and-yellow-flower-with-green-stems-36764/

The service also supports ad-hoc conversion of Html to PDF.

Simply POST to http://localhost/api/html2pdf with the body containing of the HTML to convert into PDF.

##### Hosting on Azure
---

TODO

##### Development
---

As .Net Core 2.1 is still in development, the .Net Core 2.1 Preview SDK must be downloaded and installed. 

The latest version of Chromium available for Alpine at the time of this writing is Chrome 64.