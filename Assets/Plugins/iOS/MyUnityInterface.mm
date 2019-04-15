//
//  UnityInterface.m
//  Unity-iPhone
//
//  Created by haobo on 2019/4/1.
//

#import <Foundation/Foundation.h>
#import "MyUnityInterface.h"

@interface MyUnityInterface()
@property (nonatomic, strong)UIImpactFeedbackGenerator* feedbackGenerator;
@end

@implementation MyUnityInterface

static MyUnityInterface *unityInstance = nil;

- (id) init {
    //@synchronized (self) {
        if (self = [super init]){
            self.feedbackGenerator = [[UIImpactFeedbackGenerator alloc] init];
            [self.feedbackGenerator prepare];
        }
    //}
    return self;
}

- (void) dealloc {
    self.feedbackGenerator = NULL;
}

+ (MyUnityInterface*) sharedInstance {
    @synchronized (self) {
        if(unityInstance == nil){
            unityInstance = [[self alloc] init];
            //[unityInstance retain];
        }
    }
    return unityInstance;
}


- (void) callTapticImpact {
    NSLog(@"callTapticImpact");
    [self.feedbackGenerator impactOccurred];
}

@end



extern "C" {
    
    void SUN_TapTicImpact(){
        NSLog(@"SUN_TapTicImpact");
        [[MyUnityInterface sharedInstance] callTapticImpact];
        //[unityInstance  callTapticImpact];
    }
    
}// end of extern "C"
