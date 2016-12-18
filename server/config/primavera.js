const username = 'sinf'
const password = 'primavera'

module.exports.url = 'http://localhost:52313'
module.exports.auth = 'Basic ' + new Buffer(username + ':' + password).toString('base64');