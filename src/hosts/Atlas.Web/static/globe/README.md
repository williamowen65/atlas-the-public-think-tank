# Globe map asset

`land.geojson` is Natural Earth's 1:110m land data, vendored so the globe makes no runtime CDN or external map requests.

Source: https://github.com/nvkelso/natural-earth-vector/blob/master/geojson/ne_110m_land.geojson

Natural Earth data is public domain; its terms say attribution is not required. The optional in-app credit lives in the globe information disclosure: https://www.naturalearthdata.com/about/terms-of-use/

The D3 orthographic globe and live pin interaction adapt William Owen's GeographicPinMap reference at commit `68febc15e1b9f0509445339744f9e7c07def16c2` (ISC): https://github.com/williamowen65/GeographicPinMap
