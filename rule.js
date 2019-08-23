const fs = require('fs')
// 免安装证书，需要合法ssl证书
// var sslServer = ''
module.exports = {
  summary: 'ModifyBH by heqyou_free',
  * beforeSendRequest (requestDetail) {
    if (requestDetail.url.indexOf('_compressed/DataVersion.unity3d') > -1) {
      if (requestDetail.url.indexOf('android') > -1) {
        return returnFileAsResponse('a_DataVersion.unity3d')
      }
      if (requestDetail.url.indexOf('iphone') > -1) {
        return returnFileAsResponse('i_DataVersion.unity3d')
      }
    }
    if (requestDetail.url.indexOf('excel_output_') > -1) {
      if (requestDetail.url.indexOf('android') > -1) {
        return returnFileAsResponse('a_excel_output.unity3d')
      }
      if (requestDetail.url.indexOf('iphone') > -1) {
        return returnFileAsResponse('i_excel_output.unity3d')
      }
    }
    if (requestDetail.url.indexOf('setting_') > -1) {
      if (requestDetail.url.indexOf('android') > -1) {
        return returnFileAsResponse('a_setting.unity3d')
      }
      if (requestDetail.url.indexOf('iphone') > -1) {
        return returnFileAsResponse('i_setting.unity3d')
      }
    }
  },
  * beforeSendResponse (requestDetail, responseDetail) {
    // 免安装证书，要把beforeSendRequest注释掉
    // if (requestDetail.url.indexOf('query_gameserver') > -1) {
    //   console.log('requested gameserver')
    //   var newResponse = Object.assign({}, responseDetail.response)
    //   var json = JSON.parse(newResponse.body)
    //   json.asset_boundle_url = json.asset_boundle_url.replace('bundle.bh3.com', sslServer)
    //   var str = JSON.stringify(json)
    //   newResponse.body = str
    //   return {
    //     response: newResponse
    //   }
    // }
  }
}
function returnFileAsResponse (name) {
  console.log(`requested ${name}`)
  var resp = fs.readFileSync(`${name}`)
  return {
    response: {
      statusCode: 200,
      header: {
        'Accept-Ranges': 'bytes',
        'Content-Type': 'application/octet-stream'
      },
      body: resp
    }
  }
}
