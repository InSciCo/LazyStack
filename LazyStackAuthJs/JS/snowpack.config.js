module.exports = {
    buildOptions: {
        out: '../wwwroot/js/',
        clean: true
    },
    packageOptions: {
        polyfillNode: true
    },
    mount: {
        'src': '/'
    },
};